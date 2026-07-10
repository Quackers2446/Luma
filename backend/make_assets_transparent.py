import os
from PIL import Image
from collections import deque

def remove_background_floodfill(filepath):
    try:
        img = Image.open(filepath)
        img = img.convert("RGBA")
        width, height = img.size
        pixels = img.load()

        # Visited set to keep track of background pixels
        visited = set()
        queue = deque()

        # Seed the queue with all edge pixels (corners and borders)
        for x in range(width):
            queue.append((x, 0))
            queue.append((x, height - 1))
        for y in range(1, height - 1):
            queue.append((0, y))
            queue.append((width - 1, y))

        # Check if a pixel is "close to white"
        def is_white(rgba):
            r, g, b, a = rgba
            # Check if RGB values are all above 240
            return r > 240 and g > 240 and b > 240

        # Flood fill search
        while queue:
            x, y = queue.popleft()
            if (x, y) in visited:
                continue

            if 0 <= x < width and 0 <= y < height:
                rgba = pixels[x, y]
                if is_white(rgba):
                    visited.add((x, y))
                    # Add 4-way neighbors
                    for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                        nx, ny = x + dx, y + dy
                        if (nx, ny) not in visited:
                            queue.append((nx, ny))

        # Convert visited background pixels to fully transparent alpha
        for x, y in visited:
            pixels[x, y] = (255, 255, 255, 0)

        img.save(filepath, "PNG")
        print(f"Processed: {os.path.basename(filepath)} (transparent background applied)")
    except Exception as e:
        print(f"Failed to process {filepath}: {e}")

def main():
    assets_dir = "/Users/quackers/dev/Luma/backend/public/assets"
    if not os.path.exists(assets_dir):
        print(f"Directory not found: {assets_dir}")
        return

    for filename in os.listdir(assets_dir):
        if filename.endswith(".png"):
            filepath = os.path.join(assets_dir, filename)
            remove_background_floodfill(filepath)

if __name__ == "__main__":
    main()
