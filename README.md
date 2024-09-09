# Samwise Dialogue System

**Samwise™** is a game development tool for writing interactive dialogues.

The objective of this tool is to empower any writer or designer to intricately craft branching narratives, offering extensive control over dialogue scripting to create truly natural interactions essential for immersive gaming experiences.

**Samwise™** consists of three components:
1. A scripting language for writing interactive dialogues;
2. A runtime library for parsing such files and supporting the execution of dialogues in a videogame (or any other interactive software);
3. A Visual Studio Code extension that assists the developer in the task of writing dialogues and testing them quickly.
   
## Features

These are the main language features:
- Text lines and captions
- Branching narrative
- Variables and Conditional blocks
- Labels and Gotos
- Time-based choices
- Parallel and interruptible dialogues (Fork, Join, Cancel nodes)
   - (e.g. Oxenfree-like narrative)
- Asynchronous flow (Await node)
   - It can be used to design your narrative top-down, reuse text lines, etc. 
- Challenge Checks
   - (e.g. Disco Elysium-like skill checks)
- Metadata (IDs, tags, comments)
- Custom code

## A Basic Dialogue in Samwise

```samwise
* Bob and Alice are discussing where to go for dinner.

Alice> Where do you want to go for dinner tonight?
Bob:
    - I'd like to go to the Japanese restaurant
        Alice> Do you prefer an all-you-can-eat buffet or a traditional restaurant?
        Bob:
            -- All-you-can-eat
                Bob> I prefer an all-you-can-eat buffet; I'm very hungry and don't want to spend a fortune.

            -- Traditional
                Bob> I prefer a traditional restaurant; we might pay more, but the quality is usually better.
        Alice> All right then.
```

## Documentation

[Here](https://davidebarbieri.github.io/samwise/) you can find the full documentation of this tool.

## Unity Plug-in

[Here](https://github.com/davidebarbieri/samwiseUnity) you can find the Unity plug-in repository.

## How can you help me

Please consider to support my work on [Github Sponsors](https://github.com/sponsors/davidebarbieri)!

I also appreciate all feedback, bug reports, and user suggestions regarding the use of Samwise. Your insights are invaluable in improving its functionality and user experience. 

However, please note that Samwise is still a personal project, and as such, **I do not accept pull requests**. While I am delighted to see Samwise being utilized in various video game projects, it remains a project driven by personal passion. Therefore, if you have suggestions for improvement, I kindly ask that you specify what aspect would you need to be enhanced rather than providing direct code contributions. This approach ensures that any modifications align with the original vision and maintains the enjoyment of the project for its creator.

## Licensing

**Samwise™** is free to be used in commercial and non-commercial products, but it requires you to attribute the work in the manner specified in the [License](LICENSE.md). For example, in order to be used in a video game it's enough to include the ["written in Samwise" logo](docs/logo/samwise-both-large.png) and/or the acknowledgment "Samwise Library™ by Davide 'PeevishDave' Barbieri" in the credits screen.

**Samwise™**'s license covers the description of the language and the runtime library, but it doesn't extend on the dialogues you write in that language. Your narrative is, of course, totally your property. 


# Acknowledgements

Even if Samwise is a text-based tool, I must acknowledge that the design of Samwise was consciously and/or unconsciously conditioned by using [Outspoken](https://www.demigiant.com/plugins/outspoken/), an awesome node-based visual editor made by **Daniele 'Demigiant' Giardini**.

I want to thank **Florent Maurin** for being the first user of Samwise, for trusting me to use my tool in one of his video games, and for giving me a lot of valuable feedback that definitely improved the quality of this tool for anyone.
