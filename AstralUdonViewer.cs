﻿using MelonLoader;
using System;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using VRC.Udon;

[assembly: MelonInfo(typeof(Astrum.AstralUdonViewer), nameof(Astrum.AstralUdonViewer), "0.3.0", downloadLink: "github.com/Astrum-Project/" + nameof(Astrum.AstralUdonViewer))]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace Astrum
{
    public partial class AstralUdonViewer : MelonMod
    {
        private const string basePath = "UserData/AstralUdonDecompiler";

        public override void OnApplicationStart()
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
        }

        public override void OnSceneWasLoaded(int index, string name)
        {
            if (index == -1)
                MelonCoroutines.Start(DissassembleAll());
        }

        public static System.Collections.IEnumerator DissassembleAll()
        {
            string path = $"{basePath}/{RemoveInvalid(SceneManager.GetActiveScene().name)}";

            if (Directory.Exists(path)) yield break;
            else Directory.CreateDirectory(path);

            while (VRC.SDKBase.Networking.LocalPlayer is null)
                yield return null;

            // todo: change this to All
            UdonBehaviour[] behaviours = UnityEngine.Object.FindObjectsOfType<UdonBehaviour>()
                .GroupBy(x => x.Pointer)
                .Select(x => x.FirstOrDefault())
                .ToArray();

            if (behaviours.Length == 0) yield break;

            AstralCore.Logger.Notif($"[UdonViewer] Disassembling {behaviours.Length} UdonBehaviours");

            foreach (UdonBehaviour behaviour in behaviours)
            {
                if (behaviour?._program is null) continue;

                AstralCore.Logger.Trace($"Disassembling {behaviour.name}");
                MelonCoroutines.Start(Disassembler.DisassembleProgram($"{path}/{RemoveInvalid(behaviour.name)}.uasm", behaviour));

                yield return null;
            }

            // this is a lie! it just started the last behaviour, not finished at all
            AstralCore.Logger.Notif("[UdonViewer] Finished!");
        }

        private static string RemoveInvalid(string str)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                str = str.Replace(c, '_');
            return str;
        }
    }
}
