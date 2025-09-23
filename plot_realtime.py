import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation

# Path to Unity data file
file_path = "C:/Users/mkarim1/Desktop/My Unity Projects/SenseGlove-and-Planetary-movement-main/SenseGlove-and-Planetary-movement-main/Assets/data.txt"

data_list = []

# Create figure and line
fig, ax = plt.subplots()
fig.suptitle("Vibration amplitude")  # Set figure title
line, = ax.plot([], [])

# Parameters
max_points = 500        # Keep last 500 points (x-axis longer)
update_interval = 100   # Update every 100 ms

def init():
    ax.set_xlim(0, max_points)
    ax.set_ylim(-1.5, 1.5)  # Adjust based on your data range
    ax.set_xlabel("Time (frames)")
    ax.set_ylabel("Amplitude")
    return line,

def update(frame):
    global data_list
    try:
        # Read all lines from file
        with open(file_path, 'r') as f:
            lines = f.readlines()
        # Convert to float
        data_list = [float(line.strip()) for line in lines if line.strip()]
        # Keep last max_points
        if len(data_list) > max_points:
            data_list = data_list[-max_points:]

        line.set_data(range(len(data_list)), data_list)
        # Update x-axis dynamically
        ax.set_xlim(0, max(len(data_list), max_points))
    except:
        pass
    return line,

ani = FuncAnimation(fig, update, init_func=init, blit=True, interval=update_interval)
plt.show()
