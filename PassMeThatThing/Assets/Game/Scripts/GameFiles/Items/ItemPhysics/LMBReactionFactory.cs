using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;

public static class LMBReactionFactory
{
    private static readonly Dictionary<string, Func<PhysicalItem, LMBReaction>> Registry = 
        new Dictionary<string, Func<PhysicalItem, LMBReaction>>()
        {
            { "wrench", item => new LMBWrench(item) },
            { "wirecutters", item => new LMBWirecutters(item) },
            //{},
            { "flashlight", item => new LMBFlashlight(item) },
            { "wirecutters", item => new LMBFlashlight(item) },
        };

    public static LMBReaction CreateReaction(string id, PhysicalItem item)
    {
        if (Registry.TryGetValue(id, out var createFunc))
        {
            return createFunc(item);
        }

        Debug.LogError($"Reaction ID {id}");
        return null;
    }
}