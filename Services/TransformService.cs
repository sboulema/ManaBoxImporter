using ManaBoxImporter.Models.Inventory;
using ManaBoxImporter.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManaBoxImporter.Services;

public class TransformService(
    ScryfallRepository scryfallRepository,
    SeventeenLandsRepository seventeenLandsRepository)
{
    public async Task<List<InventoryCard>> TransformInventory(InventoryModel inventory)
    {
        var scryfallCards = await scryfallRepository.GetCards();

        if (scryfallCards?.Any() != true)
        {
            Console.WriteLine("Unable to load Scryfall database");
            return [];
        }

        var cards17Lands = await seventeenLandsRepository.Get17LandsCards();

        if (cards17Lands?.Any() != true)
        {
            Console.WriteLine("Unable to load 17Lands database");
            return [];
        }

        var collection = new List<InventoryCard>();

        collection = [.. inventory.Cards
        .Join(cards17Lands,
            collectionCard => collectionCard.GroupId,
            card17Lands => card17Lands.ArenaId,
            (collectionCard, cards17Lands) => new InventoryCard
            {
                GroupId = collectionCard.GroupId,
                Name = cards17Lands.Name,
                Quantity = collectionCard.Quantity,
            })
        .GroupJoin(
            scryfallCards,
            collectionCard => collectionCard.GroupId,
            scryfallCards => scryfallCards.ArenaId,
            (collectionCard, scryfallCard) => new
            {
                CollectionCard = collectionCard,
                ScryfallCard = scryfallCard,
            })
        .SelectMany(
            cardMatch => cardMatch.ScryfallCard.DefaultIfEmpty(),
            (cardMatch, scryfallCard) => new InventoryCard
            {
                GroupId = cardMatch.CollectionCard.GroupId,
                Name = cardMatch.CollectionCard.Name,
                Quantity = cardMatch.CollectionCard.Quantity,
                SetCode = scryfallCard?.SetCode ?? string.Empty,
                SetName = scryfallCard?.SetName ?? string.Empty,
                CollectorNumber = scryfallCard?.CollectorNumber ?? string.Empty,
                ScryFallId = scryfallCard?.Id,
            }
        )
        .Where(card => !IsAlchemy(card))];

        var alchemyCollection = collection
            .Where(card => card.ScryFallId == null)
            .Join(scryfallCards.Where(scryfallCard => scryfallCard.SetType == "alchemy"),
                collectionCard => collectionCard.Name,
                scryfallCard => scryfallCard.Name,
                (collectionCard, scryfallCard) => new InventoryCard
                {
                    GroupId = collectionCard.GroupId,
                    Name = collectionCard.Name,
                    Quantity = collectionCard.Quantity,
                    SetCode = scryfallCard.SetCode,
                    SetName = scryfallCard.SetName,
                    CollectorNumber = scryfallCard.CollectorNumber,
                    ScryFallId = scryfallCard.Id,
                });

        collection = [.. collection
            .Where(collectionCard => collectionCard.ScryFallId != null)
            .Concat(alchemyCollection)
            .DistinctBy(collectioncard => collectioncard.ScryFallId)];

        return collection;
    }

    private static bool IsAlchemy(InventoryCard card)
        => card.Name.StartsWith("A-", StringComparison.OrdinalIgnoreCase);
}
