"""Builds the HUD icons and frames of the 0.3.1 style into svg/ (T-065.1).

Icons: a silhouette from game-icons.net (CC BY 3.0, sources and authors in
game-icons/ and Assets/UI/Icons/LICENSE-game-icons.txt), filled with a
two-tone colour of the style, a dark outline and a soft shadow, so it reads
on any ground at 40 px. Frames, rims and the level badge are drawn here.

Usage: python make_icons.py   then   python render.py
"""
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(HERE, 'game-icons')
OUT = os.path.join(HERE, 'svg')

# The style in numbers that the pictures need. Colours of the theme live in
# Assets/UI/Theme/theme.uss; these are their twins for the baked pictures.
OUTLINE = '#04070b'
RIM = '#6e93aa'
RIM_LIGHT = '#a9cadb'
FIELD_IN = '#172430'
FIELD_OUT = '#0a1018'
ACCENT = '#4fe0ff'
ACCENT_CORE = '#c8f6ff'
XP = '#e9c25a'

# Silhouette tones: top colour, bottom colour.
WHITE = ('#f2f8fb', '#b5c8d4')
GREY = ('#8b979f', '#5b666e')

# name: (source, tone)
ICONS = {
    'icon_sword': ('lorc/broadsword', WHITE),
    'icon_hold': ('lorc/checked-shield', WHITE),
    'icon_gather': ('lorc/axe-in-stump', WHITE),
    'icon_build': ('lorc/hammer-nails', WHITE),
    'res_gold': ('delapouite/two-coins', ('#ffe9a3', '#e0a634')),
    'res_wood': ('delapouite/wood-pile', ('#ecc08e', '#a8703e')),
    'res_food': ('lorc/meat', ('#f6b6a0', '#c76a50')),
    'skill_blink': ('lorc/teleport', ('#d8e2ff', '#7f95ff')),
    'skill_delayed_explosion': ('lorc/time-bomb', ('#ffd2b0', '#ff7a45')),
    'skill_light_orb': ('lorc/ball-glow', ('#fff6c8', '#ffd65c')),
    'skill_magic_bolt': ('delapouite/bolt-spell-cast', ('#ecd6ff', '#a46bff')),
    'skill_poisonous_attack': ('lorc/dripping-blade', ('#f6c0ee', '#c252b5')),
    'skill_shock': ('lorc/lightning-arc', ('#e4f2ff', '#6fb4ff')),
    'skill_stun_bolt': ('skoll/knockout', ('#fff0bc', '#ffb83d')),
    'skill_water_wave': ('lorc/big-wave', ('#c8f1ff', '#3fa9e8')),
}


def silhouette(source):
    """The white path(s) of a game-icons SVG, without its black backdrop."""
    svg = open(os.path.join(SRC, source + '.svg'), encoding='utf-8').read()
    paths = re.findall(r'<path[^>]*?d="([^"]+)"', svg)
    return [d for d in paths if d.strip() not in ('M0 0h512v512H0z',)]


def outline_filter(fid, grow, shadow):
    return f'''<filter id="{fid}" x="-25%" y="-25%" width="150%" height="150%">
      <feMorphology in="SourceAlpha" operator="dilate" radius="{grow}" result="grown"/>
      <feFlood flood-color="{OUTLINE}" flood-opacity="0.95"/>
      <feComposite in2="grown" operator="in" result="outline"/>
      <feOffset in="outline" dy="{shadow}" result="moved"/>
      <feGaussianBlur in="moved" stdDeviation="{shadow}" result="shadow"/>
      <feComponentTransfer in="shadow" result="soft"><feFuncA type="linear" slope="0.6"/></feComponentTransfer>
      <feMerge><feMergeNode in="soft"/><feMergeNode in="outline"/><feMergeNode in="SourceGraphic"/></feMerge>
    </filter>'''


def icon_svg(paths, tone, extra=''):
    top, bottom = tone
    body = ''.join(f'<path fill="url(#tone)" d="{d}"/>' for d in paths)
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 512 512">
  <defs>
    <linearGradient id="tone" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="{top}"/>
      <stop offset="1" stop-color="{bottom}"/>
    </linearGradient>
    {outline_filter('o', 14, 8)}
  </defs>
  <g filter="url(#o)">
    <g transform="translate(56 56) scale(0.78125)">{body}</g>
    {extra}
  </g>
</svg>
'''


def frame_svg(size, rim_width, hairline, tick):
    """A flat dark disc with a thin steel rim and four tactical notches."""
    ticks = ''.join(
        f'<rect x="{128 - tick[0] / 2}" y="2" width="{tick[0]}" height="{tick[1]}" rx="{tick[0] / 2}" '
        f'fill="{RIM_LIGHT}" transform="rotate({a} 128 128)"/>'
        for a in (0, 90, 180, 270))
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="{size}" height="{size}" viewBox="0 0 256 256">
  <defs>
    <radialGradient id="field" cx="0.5" cy="0.42" r="0.6">
      <stop offset="0" stop-color="{FIELD_IN}"/>
      <stop offset="1" stop-color="{FIELD_OUT}"/>
    </radialGradient>
  </defs>
  <circle cx="128" cy="131" r="126" fill="#000" opacity="0.45"/>
  <circle cx="128" cy="128" r="{124 - rim_width / 2}" fill="url(#field)" stroke="{RIM}" stroke-width="{rim_width}"/>
  <circle cx="128" cy="128" r="{124 - rim_width - 7}" fill="none" stroke="{RIM_LIGHT}" stroke-width="{hairline}" opacity="0.35"/>
  {ticks}
</svg>
'''


def glow_svg():
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256">
  <defs>
    <filter id="blur" x="-30%" y="-30%" width="160%" height="160%">
      <feGaussianBlur stdDeviation="8"/>
    </filter>
  </defs>
  <circle cx="128" cy="128" r="106" fill="none" stroke="{ACCENT}" stroke-width="18" filter="url(#blur)" opacity="0.9"/>
  <circle cx="128" cy="128" r="106" fill="none" stroke="{ACCENT}" stroke-width="5"/>
  <circle cx="128" cy="128" r="106" fill="none" stroke="{ACCENT_CORE}" stroke-width="2"/>
</svg>
'''


def badge_svg():
    hexagon = 'M48 5 L85 26.5 L85 69.5 L48 91 L11 69.5 L11 26.5 Z'
    inner = 'M48 15 L76 31.5 L76 64.5 L48 81 L20 64.5 L20 31.5 Z'
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96" viewBox="0 0 96 96">
  <path d="{hexagon}" fill="#000" opacity="0.45" transform="translate(0 2)"/>
  <path d="{hexagon}" fill="{FIELD_OUT}" stroke="{XP}" stroke-width="4" stroke-linejoin="round"/>
  <path d="{inner}" fill="none" stroke="{XP}" stroke-width="1.5" opacity="0.45" stroke-linejoin="round"/>
</svg>
'''


def core_empty_svg():
    """The empty centre of the ring for a unit with no attack (T-066): two thin
    rings, four notches and a small diamond, quiet steel on transparent."""
    notches = ''.join(
        f'<rect x="125" y="40" width="6" height="22" rx="3" fill="{RIM_LIGHT}" opacity="0.5" '
        f'transform="rotate({a} 128 128)"/>' for a in (45, 135, 225, 315))
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256">
  <circle cx="128" cy="128" r="84" fill="none" stroke="{RIM_LIGHT}" stroke-width="3" opacity="0.32"/>
  <circle cx="128" cy="128" r="52" fill="none" stroke="{RIM_LIGHT}" stroke-width="2" opacity="0.22" stroke-dasharray="6 9"/>
  {notches}
  <path d="M128 106 L150 128 L128 150 L106 128 Z" fill="none" stroke="{RIM_LIGHT}" stroke-width="3" opacity="0.45"/>
</svg>
'''


def write(name, text):
    with open(os.path.join(OUT, name + '.svg'), 'w', encoding='utf-8') as f:
        f.write(text)
    print('svg', name)


if __name__ == '__main__':
    for name, (source, tone) in ICONS.items():
        write(name, icon_svg(silhouette(source), tone))

    # No attack: the sword greyed out and struck through.
    slash = (f'<path d="M120 120 L392 392" stroke="{OUTLINE}" stroke-width="64" stroke-linecap="round"/>'
             f'<path d="M120 120 L392 392" stroke="#e3ebf0" stroke-width="34" stroke-linecap="round"/>')
    write('icon_no_attack', icon_svg(silhouette('lorc/broadsword'), GREY, slash))

    write('frame_big', frame_svg(256, 5, 2, (6, 14)))
    write('frame_small', frame_svg(128, 9, 4, (10, 22)))
    write('glow_ring', glow_svg())
    write('badge_level', badge_svg())
    write('core_empty', core_empty_svg())
