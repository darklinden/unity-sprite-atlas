#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import os
import shutil
import sys

root_path = os.path.dirname(os.path.abspath(__file__))
wtf_icon = os.path.join(root_path, 'wtf-icon.png')

des_folder = os.path.join(root_path, 'Sprites', 'Icons')


def random_name():
    import random
    import string
    return random.sample(string.ascii_letters, 1)[0] + (''.join(random.sample(string.ascii_letters + string.digits, 7)))


def main():

    folders = os.listdir(des_folder)

    for folder in folders:

        folder_path = os.path.join(des_folder, folder)
        if not os.path.isdir(folder_path):
            continue

        # loop 10 times to fill the folder
        for i in range(10):
            rand_name = folder + '_' + random_name() + '.png'
            shutil.copy(wtf_icon, os.path.join(folder_path, rand_name))


if __name__ == "__main__":
    main()
