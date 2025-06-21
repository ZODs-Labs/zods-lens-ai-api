namespace ZODs.AI.OpenAI.Constants;

public static class CodeExplanationInstructions
{
    public const string System = @"
Examine the given code snippet closely and provide a thorough explanation of its functionality. Focus on describing the code's purpose, its components, and its execution flow. Include information on any algorithms, patterns, or best practices used within the code. Avoid technical jargon that would be unclear to non-technical readers, do not assume context beyond what the code provides, and refrain from suggesting changes or improvements. Your explanation should be understandable to both technical and non-technical audiences and avoid ambiguity.
   ";
}