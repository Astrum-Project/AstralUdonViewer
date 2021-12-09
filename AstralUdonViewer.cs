using MelonLoader;
using System;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using VRC.Udon;
using VRC.Udon.UAssembly.Disassembler;

[assembly: MelonInfo(typeof(Astrum.AstralUdonDecompiler), nameof(Astrum.AstralUdonDecompiler), "0.1.0", downloadLink: "github.com/Astrum-Project/" + nameof(Astrum.AstralUdonDecompiler))]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace Astrum
{
    public class AstralUdonDecompiler : MelonMod
    {
        private const string basePath = "UserData/AstralUdonDecompiler";

        public override void OnApplicationStart()
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
        }

        public override void OnSceneWasLoaded(int index, string name)
        {
            if (index != -1) return;
            MelonCoroutines.Start(WaitForLocalLoad(name));
        }

        public System.Collections.IEnumerator WaitForLocalLoad(string name)
        {
            Scene scene = SceneManager.GetSceneByName(name);
            while (!scene.GetRootGameObjects().Any(f => f.name.StartsWith("VRCPlayer[Local]")))
                yield return null;

            DissassembleAll();
        }

        public static void DissassembleAll()
        {
            string path = $"{basePath}/{RemoveInvalid(SceneManager.GetActiveScene().name)}";

            if (Directory.Exists(path)) return;

            else Directory.CreateDirectory(path);

            UdonBehaviour[] behaviours = UnityEngine.Object.FindObjectsOfType<UdonBehaviour>();
            
            AstralCore.Logger.Debug($"{behaviours.Length} UdonBehaviours");

            for (int i = 0; i < behaviours.Length - 1; i++)
            {
                if (behaviours[i]._program is null) continue;

                File.WriteAllLines($"{path}/{RemoveInvalid(behaviours[i].name)}.uasm", DisassembleProgram(behaviours[i]));
            }
        }

        public static string[] DisassembleProgram(UdonBehaviour udonBehaviour)
        {
            AstralCore.Logger.Trace($"Disassembling {udonBehaviour.name}");
            Console.Beep(800, 10);
            return UAssemblyDisassembler.DisassembleProgram(udonBehaviour._program);
        }

        private static string RemoveInvalid(string str)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                str = str.Replace(c, '_');
            return str;
        }
    }
}
