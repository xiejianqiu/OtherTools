using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Experimental.PlayerLoop;

public class ShowPlayerLoop
{
    static void PrintPlayerLoop(PlayerLoopSystem playerLoop)
    {
        StringBuilder builder = new StringBuilder();
        for (var idx = 0; idx < playerLoop.subSystemList.Length; idx++)
        {
            var header = playerLoop.subSystemList[idx];
            builder.AppendLine($"<color=green>------{idx} {header.type.Name}------</color>");
            for (var idx2 = 0; idx2 < header.subSystemList.Length; idx2++)
            {
                var subSystem = header.subSystemList[idx2];
                builder.AppendLine($"{idx2} {header.type.Name}.{subSystem.type.Name}");
            }
        }
        Debug.LogError(builder.ToString());
    }
    //[RuntimeInitializeOnLoadMethod()]
    static void Init()
    {
        var mySystem = new PlayerLoopSystem()
        {
            type = typeof(MyLoopSystemUpdate),
            updateDelegate = () =>
            {
                UnityInvoke.PreUpdate();
            }
        };
        var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
        var updateSystem = playerLoop.subSystemList[3]; // Update
        var subSystem = new List<PlayerLoopSystem>(updateSystem.subSystemList);
        subSystem.Add(mySystem);
        updateSystem.subSystemList = subSystem.ToArray();
        playerLoop.subSystemList[3] = updateSystem;

        var idx = 0;
        List<PlayerLoopSystem> lst = new List<PlayerLoopSystem>(playerLoop.subSystemList[idx].subSystemList);
        lst.RemoveAll(s=>(s.type == typeof(Initialization.XREarlyUpdate) 
        ));
        playerLoop.subSystemList[idx].subSystemList = lst.ToArray();

        //idx = 1;
        //lst = new List<PlayerLoopSystem>(playerLoop.subSystemList[idx].subSystemList);
        //lst.RemoveAll(s => (s.type == typeof(EarlyUpdate.XRUpdate)
        //|| s.type == typeof(EarlyUpdate.DirectorSampleTime)
        //));
        //playerLoop.subSystemList[idx].subSystemList = lst.ToArray();

        //idx = 2;
        //lst = new List<PlayerLoopSystem>(playerLoop.subSystemList[idx].subSystemList);
        //lst.RemoveAll(s => (s.type == typeof(EarlyUpdate.XRUpdate)
        //|| s.type == typeof(FixedUpdate.DirectorFixedUpdate)
        //|| s.type == typeof(FixedUpdate.DirectorFixedSampleTime)
        //|| s.type == typeof(FixedUpdate.DirectorFixedUpdatePostPhysics)
        //|| s.type == typeof(FixedUpdate.XRFixedUpdate)
        //|| s.type == typeof(FixedUpdate.Physics2DFixedUpdate)
        //|| s.type == typeof(FixedUpdate.AudioFixedUpdate)
        //));
        //playerLoop.subSystemList[idx].subSystemList = lst.ToArray();

        //idx = 3;
        //lst = new List<PlayerLoopSystem>(playerLoop.subSystemList[idx].subSystemList);
        //lst.RemoveAll(s => (s.type == typeof(PreUpdate.Physics2DUpdate)
        //|| s.type == typeof(PreUpdate.WindUpdate)
        //));
        //playerLoop.subSystemList[idx].subSystemList = lst.ToArray();

        //idx = 4;
        //lst = new List<PlayerLoopSystem>(playerLoop.subSystemList[idx].subSystemList);
        //lst.RemoveAll(s => (s.type == typeof(Update.DirectorUpdate)
        //));
        //playerLoop.subSystemList[idx].subSystemList = lst.ToArray();

        //idx = 5;
        //lst = new List<PlayerLoopSystem>(playerLoop.subSystemList[idx].subSystemList);
        //lst.RemoveAll(s => (s.type == typeof(PreLateUpdate.DirectorDeferredEvaluate)
        //|| s.type == typeof(PreLateUpdate.DirectorUpdateAnimationBegin)
        //|| s.type == typeof(PreLateUpdate.DirectorUpdateAnimationEnd)
        //));
        //playerLoop.subSystemList[idx].subSystemList = lst.ToArray();

        //idx = 6;
        //lst = new List<PlayerLoopSystem>(playerLoop.subSystemList[idx].subSystemList);
        //lst.RemoveAll(s => (s.type == typeof(PostLateUpdate.DirectorLateUpdate)
        //|| s.type == typeof(PostLateUpdate.DirectorRenderImage)
        //|| s.type == typeof(PostLateUpdate.XRPostPresent)
        //));
        //playerLoop.subSystemList[idx].subSystemList = lst.ToArray();


        PlayerLoop.SetPlayerLoop(playerLoop);

        PrintPlayerLoop(playerLoop);
    }
    public struct MyLoopSystemUpdate { }
}