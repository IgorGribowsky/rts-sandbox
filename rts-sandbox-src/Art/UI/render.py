"""Renders every SVG in svg/ into a transparent PNG with headless Chrome.

Usage: python render.py [name ...]   (no names = all)
Output goes to Assets/UI/Icons/<name>.png at the size written in the SVG.
A '_gray' variant is written next to icons whose name starts with 'skill_'
or 'card_': the UI swaps to it for cooldown and disabled states.
"""
import os
import re
import subprocess
import sys

from PIL import Image, ImageOps

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(HERE, 'svg')
OUT = os.path.normpath(os.path.join(HERE, '..', '..', 'Assets', 'UI', 'Icons'))
TMP = os.path.join(HERE, '.render')
CHROME = r'C:\Program Files\Google\Chrome\Application\chrome.exe'


def render(name):
    svg_path = os.path.join(SRC, name + '.svg')
    svg = open(svg_path, encoding='utf-8').read()
    w = int(re.search(r'width="(\d+)"', svg).group(1))
    h = int(re.search(r'height="(\d+)"', svg).group(1))
    os.makedirs(TMP, exist_ok=True)
    html = os.path.join(TMP, name + '.html')
    with open(html, 'w', encoding='utf-8') as f:
        f.write('<html><body style="margin:0;background:transparent;overflow:hidden">'
                + svg + '</body></html>')
    png = os.path.join(OUT, name + '.png')
    url = 'file:///' + html.replace(os.sep, '/')
    subprocess.run([CHROME, '--headless=new', '--disable-gpu', '--hide-scrollbars',
                    '--default-background-color=00000000', '--window-size=%d,%d' % (w, h),
                    '--screenshot=' + png, url],
                   check=True, capture_output=True)
    if name.startswith('skill_') or name.startswith('card_'):
        im = Image.open(png).convert('RGBA')
        gray = ImageOps.grayscale(im.convert('RGB')).convert('RGBA')
        gray.putalpha(im.getchannel('A'))
        gray.save(os.path.join(OUT, name + '_gray.png'))
    print('ok', name, w, h)


if __name__ == '__main__':
    os.makedirs(OUT, exist_ok=True)
    names = sys.argv[1:] or [f[:-4] for f in sorted(os.listdir(SRC)) if f.endswith('.svg')]
    for n in names:
        render(n)
