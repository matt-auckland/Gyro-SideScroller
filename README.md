#Gyro Sidescroller
A simple game for the Raspberry Pi2 (With "Sense Hat"!!).

Avoid the red dots by tilting the Pi to move the green dot up and down.
1 point for each red dot that passes across the screen, game speeds up every 5 points.
Game over screen can be ended by clicking the joystick.

Relies almost entirely on the 'RPi.SenseHat' project which can be found here: github.com/emmellsoft/RPi.SenseHat


### RPi.SenseHat
A complete Windows IoT class library for the Raspberry Pi "Sense HAT" (C#)

The solution contains the following projects:
*) Rpi.SenseHat
*) RPi.SenseHat.Demo
*) RPi.SenseHat.Tools
*) RT.IoT.Sensors

The Rpi.SenseHat is the main library. It contains a nice API to the Raspberry Sense HAT in C#.
The Rpi.SenseHat is dependent on the RT.IoT.Sensors project, which is a library for managing the sensor readings from the Sense HAT. That project is currently a copy of another github repository until no NuGet of it is available.

The RPi.SenseHat.Demo project is an application that you can run on the Raspberry Pi 2. It doesn't utilize the regular UI, so there is no need to connect it to a monitor using the HDMI port.
The application comes with a number of demos.
You must choose what demo to run by modifying the code in the "DemoSelector" class. It should be fairly obvious what to do there. :-)


The RPi.SenseHat.Tools is a regular Windows console application that was used to test out some of the calculations that was needed in the actual library.
It also contains the process of converting a bitmap holding a font image into a "compiled" byte array that can be used by the font classes of the Sense HAT library.


************************
To get started:

*) Open the solution in Visual Studio 2015.

*) Make sure the "RPi.SenseHat.Demo" project is the start-up project.

*) Choose "ARM" as the solution platform.

*) Direct the debugging to a "Remote Machine" -- and make sure you enter the IP-address of your Raspberry Pi (and no authentication should be used).

*) Edit the DemoSelector class (in the root of the "RPi.SenseHat.Demo" project) to select which demo to run.

*) Run!