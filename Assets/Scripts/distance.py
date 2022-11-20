import math

G = 6.674e-11
M_E = 5.9722e24
EARTH_TO_MOON = 384400e3

def distance(G, Mass, Min = 0.1) -> float:
    return math.sqrt((G * Mass) / Min)

def main():
    print(distance(G, M_E))

if __name__ == '__main__':
    main()