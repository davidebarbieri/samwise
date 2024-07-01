using System;
using System.Collections.Generic;

namespace Peevo.Samwise.Wasm
{
    public static class IDialogueUtils
    {
        public static void AssignUniqueIDs(this Dialogue dialogue, HashSet<string> uniqueIDs)
        {
            foreach (var content in dialogue.FindTextContent(true))
            {
                var id = content.GetID();

                if (id == null)
                {
                    var preamble = content.GenerateUidPreamble(dialogue);
                    
                    int tries = 1;
                    do 
                    {
                        id = preamble + tries++.ToString("000");
                    } while (!uniqueIDs.Add(id));

                    content.SetID(id);
                }
            }
        }
        
        public static IEnumerable<IContent> AssignUniqueIDsSteppedContent(this Dialogue dialogue, HashSet<string> uniqueIDs)
        {
            // Add to all content
            foreach (var content in dialogue.FindContent())
            {
                var id = content.GetID();

                if (id == null)
                {
                    var preamble = content.GenerateUidPreamble(dialogue);
                    
                    int tries = 1;
                    do 
                    {
                        id = preamble + tries++.ToString("000");
                    } while (!uniqueIDs.Add(id));

                    content.SetID(id);
                    yield return content;
                }
            }

            // Other nodes
            foreach (var content in dialogue.Traverse())
            {
                var id = content.GetID();

                if (id == null)
                {
                    var preamble = content.GenerateUidPreamble(dialogue);
                    
                    int tries = 1;
                    do 
                    {
                        id = preamble + tries++.ToString("000");
                    } while (!uniqueIDs.Add(id));

                    content.SetID(id);
                    yield return content;
                }
            }
        }

        public static IEnumerable<IContent> AssignUniqueIDsSteppedText(this Dialogue dialogue, HashSet<string> uniqueIDs)
        {
            foreach (var content in dialogue.FindTextContent(true))
            {
                var id = content.GetID();

                if (id == null)
                {
                    var preamble = content.GenerateUidPreamble(dialogue);
                    
                    int tries = 1;
                    do 
                    {
                        id = preamble + tries++.ToString("000");
                    } while (!uniqueIDs.Add(id));

                    content.SetID(id);
                    yield return content;
                }
            }
        }

        public static IEnumerable<IContent> ReplaceAnonymousVariablesStepped(this Dialogue dialogue, HashSet<string> uniqueVariables)
        {
            foreach ((IContent content, List<IStatefulElement> elements) i in dialogue.FindAnonymousContent())
            {
                foreach (var element in i.elements)
                {
                    // Replace
                    element.StateVariableContext = "__gen." + dialogue.Label + ".";
                    
                    element.UsesAnonymousVariable = false;

                    var initialName = element.StateVariableName.Substring(0, element.StateVariableName.LastIndexOf('_') + 1);

                    int tries = 1;
                    while (!uniqueVariables.Add(element.StateVariableContext + element.StateVariableName)) 
                    {
                        // Try another name
                        element.StateVariableName = initialName + tries++.ToString("000");
                    } 
                }

                yield return i.content;
            }
        }
    }
}