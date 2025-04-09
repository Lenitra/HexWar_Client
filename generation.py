import math

def axial_to_world_flat(q, r, size):
    """Convert axial hex coordinates to world space for flat-top hexes."""
    x = size * 3/2 * q
    z = size * math.sqrt(3) * (r + q / 2)
    return (round(x, 4), round(z, 4))

# Axial directions in flat-top layout
DIRECTIONS = [
    (1, 0), (1, -1), (0, -1),
    (-1, 0), (-1, 1), (0, 1)
]

def generate_ring_centers(radius, size=1.0, y=0.2):
    if radius == 0:
        return [f"Vector3(0f, {y}f, 0f)"]

    points = []

    # Start from the westernmost hex on the ring
    q = -radius
    r = 0

    for dir in range(6):
        dq, dr = DIRECTIONS[dir]
        for _ in range(radius):
            x, z = axial_to_world_flat(q, r, size)
            points.append(f"Vector3({x}f, {y}f, {z}f)")
            q += dq
            r += dr

    return points

# --- Test pour rayon 1 ---
radius = 2
result = generate_ring_centers(radius)

print(f"case {radius}:")
for line in result:
    print(f"    points.Add(new {line});")
print("")