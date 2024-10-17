# Metadata

!> [ This page is under development ]

## Metadata: Tags

Meaningful metadata can be incorporated into dialogue nodes by utilizing tags.

```samwise
character> This is a random line # tag1, tag2, "This is a complex tag 1", "complex tag 2"
```

 Tags, such as #happy and #skippable, serve as markers that can be leveraged by the game's code to implement tailored behaviors associated with specific nodes. For instance, these tags may trigger unique character reactions or prompt specific in-game events. 
 
 Additionally, tags offer a valuable space for insights related to a dialogue line. Use double quoted tags to provide voiceover tips, offer guidance for effective delivery, or even explain the nuances of a particular word to assist translators.

Tags can be added to Dialogue Titles block too:
```samwise
§ Title # tag1, tag2, "Comment"
```
To be honest, tags can be added to virtually everything, including options.

It's also possible to assign names to tags, allowing for better specialization or even just making them easier to track via code. Here is the syntax:
```samwise
character> Where are my panties?! # normal_tag, face=angry, voice_comment="The character here is very upset"
```
### Node Identifier

There's a unique named tag titled **"id"** designated as the default identifier for each node. This identifier is essential for tracking the corresponding line in a different language or locating the voiceover audio file for that line.