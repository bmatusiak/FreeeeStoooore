
using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace FreeeeStoooore
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class FreeeeStoooorePlugin : BaseUnityPlugin
    {
        // Mod Details
        private const string modGUID = "bmatusiask.FreeeeStoooore";
        private const string modName = "FreeeeStoooore";
        private const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static FreeeeStoooorePlugin Instance { get; private set; }
        public ManualLogSource Log { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            Log = BepInEx.Logging.Logger.CreateLogSource("UnknownMod");
            Log.LogInfo("(UnknownMod): Patching");

            harmony.PatchAll(typeof(MoonPricePatch));
        }

    }

    [HarmonyPatch(typeof(Terminal))]
    internal class MoonPricePatch
    {
        private static Terminal terminal = null;
        private static int totalCostOfItems = -5;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void FindTerminal()
        {
            FreeeeStoooorePlugin.Instance.Log.LogInfo("Finding terminal object!");
            terminal = GameObject.FindObjectOfType<Terminal>();

            if (terminal == null)
                FreeeeStoooorePlugin.Instance.Log.LogError("Failed to find terminal object!");
            else
                FreeeeStoooorePlugin.Instance.Log.LogInfo("Found terminal object!");

        }

        [HarmonyPatch("LoadNewNode")]
        [HarmonyPrefix]
        static void LoadNewNodePatchBefore(ref TerminalNode node)
        {
            if (terminal == null && node == null)
                return;

            if (node.buyRerouteToMoon != -2)
                return;

            Traverse totalCostOfItemsRef = Traverse.Create(terminal).Field("totalCostOfItems");
            totalCostOfItems = (int)totalCostOfItemsRef.GetValue();
            totalCostOfItemsRef.SetValue(0);
        }

        [HarmonyPatch("LoadNewNode")]
        [HarmonyPostfix]
        static void LoadNewNodePatchAfter(ref TerminalNode node)
        {
            if (terminal == null && node == null)
                return;

            if (totalCostOfItems == -5)
                return;

            Traverse totalCostOfItemsRef = Traverse.Create(terminal).Field("totalCostOfItems");
            totalCostOfItemsRef.SetValue(0);

            totalCostOfItems = -5;
        }

        [HarmonyPatch("LoadNewNodeIfAffordable")]
        [HarmonyPrefix]
        static void changeDecorationPrices(TerminalNode node, ref List<TerminalNode> ___ShipDecorSelection)
        {
            if (node == null)
                return;


            for (int k = 0; k < terminal.buyableItemsList.Length; k++)
            {
                terminal.buyableItemsList[k].creditsWorth = 0;
            }

            node.itemCost = 0;

        }

        [HarmonyPatch("RotateShipDecorSelection")]
        [HarmonyPostfix]
        static void changeShownDecorationPrices(ref List<TerminalNode> ___ShipDecorSelection)
        {
            ___ShipDecorSelection.Clear();

            for (int index = 0; index < StartOfRound.Instance.unlockablesList.unlockables.Count; ++index)
            {
                UnlockableItem obj = StartOfRound.Instance.unlockablesList.unlockables[index];
                if (obj.shopSelectionNode != null && !obj.alwaysInStock)
                {
                    ___ShipDecorSelection.Add(obj.shopSelectionNode);
                }
            }

            foreach (TerminalNode node in ___ShipDecorSelection)
            {
                FreeeeStoooorePlugin.Instance.Log.LogInfo("Overriding the price of ship decoration (" + node.ToString() + ")");
                node.itemCost = 0;
            }

            for (int k = 0; k < terminal.buyableItemsList.Length; k++)
            {
                terminal.buyableItemsList[k].creditsWorth = 0;
            }

        }
    }
}
