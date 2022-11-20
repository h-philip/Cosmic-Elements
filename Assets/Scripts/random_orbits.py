from __future__ import annotations
import logging

import random as rnd
from json import dumps
import string
import sys

SOLAR_MASS = 1.98847e30
SOLAR_DIAMETER = 6.957e8 * 2
# LIGHT_YEAR_TO_AU = 63241.07709
LIGHT_YEAR_TO_AU = .6324107709

MIN_MASS = 2e5
MAX_MASS = 2e32
MIN_EC = 0.0
MAX_EC = 0.8
MIN_IN = 0.0
MAX_IN = 10.0
MIN_OM = 0.0
MAX_OM = 10.0
MIN_W = 0.0
MAX_W = 10.0
MIN_MA = 0.0
MAX_MA = 10.0
MIN_A = 0.01
MAX_A = 10.0
MIN_DIAMETER = 1
MAX_DIAMETER = 100.0

MAX_DEPTH = 2
CHILD_PROBABILITY = 0.5

try:
    amount = int(sys.argv[1])
except IndexError:
    amount = 2
counter = 0
bodies = []

class Body:
    def __init__(self, attractor: Body) -> None:
        if not attractor:
            self.attractor_name = ''
            self.attractor_mass = 0.0
            self.depth = 0
        else:
            self.attractor_name = attractor.name
            self.attractor_mass = attractor.mass
            self.depth = attractor.depth + 1
        self.name = ''
        self.mass = 0
        self._ec = 0
        self._in = 0
        self._om = 0
        self._w = 0
        self._ma = 0
        self._a = 0
        self._diameter = 0

    def assign_random(self):
        if self.attractor_name == '':
            self.name = ''.join(rnd.choice(string.ascii_lowercase)
                                for x in range(8))
            self.mass = 2e31
            self._ec = 0
            self._in = 0
            self._om = 0
            self._w = 0
            self._ma = 0
            self._a = 0
            self._diameter = 100
        else:
            self.name = ''.join(rnd.choice(string.ascii_lowercase)
                                for x in range(8))
            self.mass = rnd.randrange(MIN_MASS, self.attractor.mass)
            self._ec = MIN_EC + rnd.random() * (MAX_EC - MIN_EC)
            self._in = MIN_IN + rnd.random() * (MAX_IN - MIN_IN)
            self._om = MIN_OM + rnd.random() * (MAX_OM - MIN_OM)
            self._w = MIN_W + rnd.random() * (MAX_W - MIN_W)
            self._ma = MIN_MA + rnd.random() * (MAX_MA - MIN_MA)
            self._a = MIN_A + rnd.random() * (self.attractor._a - MIN_A) if self.attractor.depth > 0 else MIN_A + \
                rnd.random() * (MAX_A - MIN_A)
            self._diameter = MIN_DIAMETER + rnd.random() * (self.attractor._diameter * .4 -
                                                            MIN_DIAMETER) if self.attractor.depth > 0 else MIN_DIAMETER + rnd.random() * (MAX_DIAMETER - MIN_DIAMETER)

    def create_children(self):
        global counter
        global amount
        if self.depth >= MAX_DEPTH:
            return
        if self.attractor_name == '':
            while counter < amount:
                counter += 1
                bodies.append(Body(self))
        else:
            while rnd.random() < CHILD_PROBABILITY and counter < amount:
                counter += 1
                bodies.append(Body(self))


    def to_dictionary(self):
        return {
            'BodyName': self.name,
            'AttractorName': self.attractor_name,
            'AttractorMass': self.attractor_mass,
            'EC': self._ec,
            'IN': self._in,
            'OM': self._om,
            'W': self._w,
            'MA': self._ma,
            'A': self._a,
            'Diameter': self._diameter
        }


class Galaxy:
    def __init__(self) -> None:
        self.planetary_systems = []
        barycenter = Body(None)
        barycenter.name = 'Galaxy barycenter'
        barycenter.mass = 2e33

        self.barycenter = barycenter

        self.logger = logging.getLogger(__name__)

    def create_planetary_systems(self, number: int):
        if number > 99:
            logging.error('%s: Number is bigger than 99',
                          Galaxy.create_planetary_systems)
        for i in range(number):
            ps = Body(self.barycenter)
            ps.name = f'planetary_system_{i:02}'
            ps.mass = rnd.uniform(SOLAR_MASS * 0.008, SOLAR_MASS * 9)
            ps._ec = rnd.uniform(0.03, 0.8)
            ps._in = rnd.uniform(0.0, 10.0)
            ps._om = rnd.uniform(0.0, 359.9)
            ps._w = rnd.uniform(0.0, 359.9)
            ps._ma = 1.0
            ps._a = rnd.uniform(0.5 * LIGHT_YEAR_TO_AU, 1e4 * LIGHT_YEAR_TO_AU)
            ps._diameter = rnd.uniform(0.5 * SOLAR_DIAMETER, 8.0 * SOLAR_DIAMETER)
            self.planetary_systems.append(ps)

    def to_dictionary(self):
        dict_list = []
        dict_list.append(self.barycenter.to_dictionary())
        for ps in self.planetary_systems:
            dict_list.append(ps.to_dictionary())
        return {
            'OrbitsData': dict_list
        }
        

g = Galaxy()
g.create_planetary_systems(9)
dictionary = g.to_dictionary()
print(dumps(dictionary, indent=2))