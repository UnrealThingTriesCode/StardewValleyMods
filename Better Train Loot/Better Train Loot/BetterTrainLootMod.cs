﻿using BetterTrainLoot.Config;
using BetterTrainLoot.Data;
using BetterTrainLoot.GamePatch;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using System.Collections.Generic;
using System.IO;

namespace BetterTrainLoot
{
    public class BetterTrainLootMod : Mod
    {
        public static BetterTrainLootMod Instance { get; private set; }
        public static int numberOfRewardsPerTrain = 0;

        internal HarmonyInstance harmony { get; private set; }

        private int maxNumberOfTrains;
        private int numberOfTrains = 0;
        private int startTimeOfFirstTrain = 600;

        internal TRAINS trainType;

        private double pctChanceOfNewTrain = 0.0;

        private bool forceNewTrain;
        private bool enableCreatedTrain = true;

        internal ModConfig config;
        internal Dictionary<TRAINS, TrainData> trainCars;

        private Railroad railroad;
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            config = helper.Data.ReadJsonFile<ModConfig>("config.json") ?? ModConfigDefaultConfig.CreateDefaultConfig("config.json");

            if (config.enableMod)
            {
                helper.Events.Input.ButtonReleased += Input_ButtonReleased;
                helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
                helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
                helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

                harmony = HarmonyInstance.Create("com.aairthegreat.mod.trainloot");
                harmony.Patch(typeof(TrainCar).GetMethod("draw"), null, new HarmonyMethod(typeof(TrainCarOverrider).GetMethod("postfix_getTrainTreasure")));

                string trainCarFile = Path.Combine("DataFiles", "trains.json");
                trainCars = helper.Data.ReadJsonFile<Dictionary<TRAINS, TrainData>>(trainCarFile) ?? TrainDefaultConfig.CreateTrainCarData(trainCarFile);

            }
        }
        private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.Y)
            {
                this.Monitor.Log("Player press Y... Choo choo");                
                forceNewTrain = true;
                enableCreatedTrain = true;
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            railroad = (Game1.getLocationFromName("Railroad") as Railroad);
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (railroad != null)
            {
                if (Game1.player.currentLocation.IsOutdoors && railroad.train.Value == null)
                {
                    if (forceNewTrain)
                    {
                        CreateNewTrain();
                    }
                    else if (enableCreatedTrain
                        && numberOfTrains < maxNumberOfTrains
                        && e.NewTime >= startTimeOfFirstTrain
                        && Game1.random.NextDouble() <= pctChanceOfNewTrain)
                    {
                        CreateNewTrain();
                    }
                }

                if (railroad.train.Value != null && !enableCreatedTrain)
                {
                    enableCreatedTrain = true;
                    trainType = (TRAINS)railroad.train.Value.type.Value;
                }
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ResetDailyValues();
            SetMaxNumberOfTrainsAndStartTime();
            UpdateTrainLootChances();            
        }

        private void CreateNewTrain()
        {
            numberOfRewardsPerTrain = 0;
            railroad.setTrainComing(config.trainCreateDelay);
            numberOfTrains++;
            forceNewTrain = false;
            trainType = TRAINS.UNKNOWN;
            this.Monitor.Log($"Setting train... Choo choo... {Game1.timeOfDay}");
            enableCreatedTrain = false;
        }

        private void ResetDailyValues()
        {
            forceNewTrain = false;
            enableCreatedTrain = true;
            numberOfTrains = 0;
            numberOfRewardsPerTrain = 0;
            pctChanceOfNewTrain = Game1.dailyLuck + config.basePctChanceOfTrain;
        }

        private void SetMaxNumberOfTrainsAndStartTime()
        {            
            switch (Game1.random.Next(0, 10))
            {
                case 0:
                    maxNumberOfTrains = 0;
                    startTimeOfFirstTrain = 2600;
                    break;
                case 1:
                case 2:
                case 3:
                    maxNumberOfTrains = 1;
                    startTimeOfFirstTrain = 1000;
                    break;
                case 4:
                case 5:
                case 6:
                    maxNumberOfTrains = 3;
                    startTimeOfFirstTrain = 800;
                    break;
                case 7:
                case 8:
                case 9:
                    maxNumberOfTrains = 5;
                    startTimeOfFirstTrain = 600;
                    break;
            }
            Monitor.Log($"Setting Max Trains to {maxNumberOfTrains}");
        }

        private void UpdateTrainLootChances()
        {
            //Update the treasure chances for today
            foreach (TrainData train in trainCars.Values)
            {
                train.UpdateTrainLootChances(Game1.dailyLuck);
            }
        }
    }
}