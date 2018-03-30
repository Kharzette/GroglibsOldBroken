# Groglibs
3D game libraries for bsp maps with static and skinned meshes and misc other stuff

This started out as an XNA 3.something project, and then ported up to 4 (which was very painful), and then ported over to SharpDX.

The BSP stuff is my take on a bsp compiler.  It started out purely as a from scratch effort, but I ran into problems with portals that led me to look at Genesis3D and eventually Quake2.  I wrote a long post about these adventures here:  https://kharza.wordpress.com/2012/01/16/fun-with-bsps-and-pre-computed-visibility/

The fact that I ported over C++ to C# of some other people's stuff complicates licensing.  I think Genesis is GPL of some sort and I forget what Quake2 was, maybe GPL as well?  I find it all terribly confusing but just be aware of all of that.  When I push up the individual libraries I'll try to note what came from where in the readmes.

All of the rest of that stuff I wrote myself, but I do use Tom Forsyth's sweet little TriLight shader stuff for character lighting.  You'll see that in the shaders.

For Tools, BSPBuilder is ... the bsp builder.

ColladaConvert handles importing Collada meshes for Statics and Characters.  Note that statics have no collision, so you'll need to match them to clip brushes.

QEntityMaker works with the quake editor quake army knife to make game entities.

Test is full of test programs.  I don't have any unit testing.
