﻿using System.Collections.Generic;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Pipliz.APIProvider.Recipes;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public class WaterbucketModEntries
  {
    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, "scarabol.waterbucket.registercallbacks")]
    public static void AfterStartup()
    {
      Pipliz.Log.Write("Loaded Waterbucket Mod 1.0 by Scarabol");
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, "scarabol.waterbucket.addrawtypes")]
    public static void AfterAddingBaseTypes()
    {
      ItemTypes.AddRawType("waterbucket",
        new JSONNode(NodeType.Object)
          .SetAs("maxStackSize", 5)
          .SetAs("npcLimit", 0)
      );
      ItemTypes.AddRawType("waterbucketfilled",
        new JSONNode(NodeType.Object)
          .SetAs("maxStackSize", 5)
          .SetAs("npcLimit", 0)
      );
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.waterbucket.registertypes")]
    public static void AfterItemTypesServer()
    {
      ItemTypesServer.RegisterOnRemove("water", WaterbucketCode.OnWaterRemoved);
      ItemTypesServer.RegisterOnAdd("waterbucketfilled", WaterbucketCode.OnAddFilled);
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, "scarabol.waterbucket.loadrecipes")]
    [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.registerrecipes")]
    public static void AfterItemTypesDefined()
    {
      RecipePlayer.AllRecipes.Add(new Recipe(new JSONNode()
        .SetAs("results", new JSONNode(NodeType.Array).AddToArray(new JSONNode().SetAs("type", "waterbucket")))
        .SetAs("requires", new JSONNode(NodeType.Array).AddToArray(new JSONNode().SetAs("type", "ironingot").SetAs("amount", 3)))
      ));
    }
  }

  static class WaterbucketCode
  {
    public static void OnWaterRemoved(Vector3Int position, ushort wasType, Players.Player causedBy)
    {
      ushort newType;
      if (World.TryGetTypeAt(position, out newType) && newType == ItemTypes.IndexLookup.GetIndex("waterbucket")) {
        ThreadManager.InvokeOnMainThread(delegate ()
        {
          ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex("air"));
          Stockpile.GetStockPile(causedBy).Add(ItemTypes.IndexLookup.GetIndex("waterbucketfilled"), 1);
        }, 0.1);
      }
    }

    public static void OnAddFilled(Vector3Int position, ushort newType, Players.Player causedBy)
    {
      ThreadManager.InvokeOnMainThread(delegate ()
      {
        Chat.SendToAll(string.Format("Someone spilled water at {0}", position));
        ServerManager.TryChangeBlock(position, ItemTypes.IndexLookup.GetIndex("water"));
        Stockpile.GetStockPile(causedBy).Add(ItemTypes.IndexLookup.GetIndex("waterbucket"), 1);
      }, 0.1);
    }
  }
}