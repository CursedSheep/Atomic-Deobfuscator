using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomicDeobfuscator
{
    class hideMethodsRemover
    {
        private static OpCode[] list = { OpCodes.Call, OpCodes.Sizeof, OpCodes.Calli };
        private static bool detect(ModuleDefMD module)
        {
            foreach (TypeDef type in module.GetTypes())
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody || !method.Body.HasInstructions || !method.Body.HasExceptionHandlers) continue;
                    var exceptionhandlers = method.Body.ExceptionHandlers.ToArray();
                    foreach (ExceptionHandler fd in exceptionhandlers)
                    {
                        if (list.Contains(fd.TryStart.OpCode) && fd.TryStart.Operand == null) return true;
                    }
                }
            }
            return false;
        }
        public static int Deobfuscate(ModuleDefMD module)
        {
            bool hasprotection = detect(module);
            int counter = 0;
            if (!hasprotection) return counter;
            foreach (TypeDef type in module.GetTypes())
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody || !method.Body.HasInstructions || !method.Body.HasExceptionHandlers) continue;
                    var exceptionhandlers = method.Body.ExceptionHandlers.ToArray();
                    foreach (ExceptionHandler fd in exceptionhandlers)
                    {
                        if (list.Contains(fd.TryStart.OpCode) && fd.TryStart.Operand == null)
                        {
                            var instructions = method.Body.Instructions;
                            //int startIndex = method.Body.Instructions.IndexOf(fd.HandlerStart);
                            int endIndex = instructions.IndexOf(fd.TryEnd);
                            for (int i = 0; i < endIndex; i++) instructions[i].OpCode = OpCodes.Nop;
                            method.Body.ExceptionHandlers.Remove(fd);
                            counter++;

                        }
                    }
                }
            }
            return counter;
        }
    }
}
