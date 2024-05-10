# Code Extensions

!> [ This page is under development ]

### Embeddable Code (Custom-defined code)

Embedded code nodes are similar to built-in code nodes, but they use double curly brackets.

!> In order to support this kind of node, the game must provide a Custom Code Parser using the Runtime API.

#### Code statements

Such statements are executed synchonously.

```
{{ external code }}
```

#### Embeddable Condition

The API allows the user to define conditions in their custom language as well.

```
[{{ external code }}]
```

#### Fork/Join/Await on Embeddable Code

In case of asynchonous code, the following syntax must be used:

```
=> {{ code }}
name => {{ code }}
<=> {{ code }}
```
forking, joining or awaiting embeddable code is similar to what happens with regular fork/join/await nodes.
The difference is that instead of executing other dialogues, custom code will be issues asynchronously.



## Fork/Join/Await on Embeddable Code

In case of asynchonous code, the following syntax must be used:

```
=> {{ code }}
name => {{ code }}
<=> {{ code }}
```
forking, joining or awaiting embeddable code is similar to what happens with regular fork/join/await nodes.
The difference is that instead of executing other dialogues, custom code will be issues asynchronously.