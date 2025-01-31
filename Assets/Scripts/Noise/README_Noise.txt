 - WHY DO WE HAVE A CLASS FOR NOISE? - 

- The idea is to derive classes off of BaseNoise, each with their own unique algorithm for generating 1D, 2D and 3D noise

- This way, we can easily modify which noise algorithm we're using for certain scripts without too much issue






 - HOW TO AND WHAT TO NOTE WHEN IMPLEMENTING A NEW NOISE ALGORITHM - 

- When using noise for any algorithm, pass the BaseNoise class into the algorithm script as a variable

- To use a new noise algorithm, simply derive a class off of BaseNoise and throw the noise code into the abstract methods

- The noise functions should ALL output a value between [0 - 1], if they do not, please modify the final output in the noise function to do so