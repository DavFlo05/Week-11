# Week 11



1\. How does each pathfinding algorithm calculate and prioritize paths?



BFS explores nearby cells using a queue and finds the shortest path when all movements have the same cost. Dijkstra's algorithm prioritizes paths with the lowest total movement cost. A\* is similar to Dijkstra's, but it also estimates the distance to the goal, making it more efficient in many situations.



2\. What challenges arise when dynamically updating obstacles in real-time?



One challenge is that adding obstacles can block the current path, forcing the algorithm to find a new one. Constantly recalculating paths can also affect game performance, especially with larger grids or many moving objects.



3\. Which algorithm should you choose and how should you adapt it for larger grids or open-world settings?



I would choose A\* because it usually finds paths more efficiently than BFS or Dijkstra's. For larger maps, I would divide the world into smaller sections and only calculate paths in areas that are needed instead of searching the entire map.



4\. What would your approach be if you were to add weighted cells (e.g., "difficult terrain" areas)?



I would assign different movement costs to each terrain type. For example, a normal road could cost 1, grass could cost 2, and mud could cost 4. I would use A\* or Dijkstra's algorithm so the character could prioritize paths with lower total costs instead of simply choosing the shortest distance.

