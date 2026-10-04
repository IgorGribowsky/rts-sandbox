"""Renders every SVG in svg/ into a transparent PNG with headless Chrome.

Usage: python render.py [name ...]   (no names = all)
Output goes to Assets/UI/Icons/<name>.png at the size written in the SVG;
sprites for the uGUI bars over units (SPRITES below) go to Assets/UI/Sprites/,
where the icon importer does not touch them and they stay 9-sliced sprites.
Grey versions for cooldown and disabled states are made by the game at run
time, so only the colour picture is stored.
"""
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(HERE, 'svg')
OUT = os.path.normpath(os.path.join(HERE, '..', '..', 'Assets', 'UI', 'Icons'))
OUT_SPRITES = os.path.normpath(os.path.join(HERE, '..', '..', 'Assets', 'UI', 'Sprites'))
SPRITES = {'bar_frame', 'bar_fill'}
# Pictures for materials in the world (grid, aim hints) go to Assets/Textures/.
OUT_TEXTURES = os.path.normpath(os.path.join(HERE, '..', '..', 'Assets', 'Textures'))
TEXTURES = {'grid_cell', 'aim_range', 'aim_area', 'aim_arrow_body', 'aim_arrow_head', 'aim_target',
            'stun_stars'}
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
    out = OUT_SPRITES if name in SPRITES else OUT_TEXTURES if name in TEXTURES else OUT
    os.makedirs(out, exist_ok=True)
    png = os.path.join(out, name + '.png')
    url = 'file:///' + html.replace(os.sep, '/')
    subprocess.run([CHROME, '--headless=new', '--disable-gpu', '--hide-scrollbars',
                    '--default-background-color=00000000', '--window-size=%d,%d' % (w, h),
                    '--screenshot=' + png, url],
                   check=True, capture_output=True)
    print('ok', name, w, h)


if __name__ == '__main__':
    os.makedirs(OUT, exist_ok=True)
    names = sys.argv[1:] or [f[:-4] for f in sorted(os.listdir(SRC)) if f.endswith('.svg')]
    for n in names:
        render(n)
