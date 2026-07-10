import os
from PIL import Image, ImageDraw

def create_transparent_canvas():
    return Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))

def save_layer(img, char_name, layer_name):
    dir_path = f"public/assets/{char_name}"
    os.makedirs(dir_path, exist_ok=True)
    img.save(f"{dir_path}/{layer_name}.png", "PNG")
    print(f"Saved: {dir_path}/{layer_name}.png")

def generate_sprout():
    char = "sprout"
    
    # 1. Shadow
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([300, 900, 724, 970], fill=(24, 20, 38, 80)) # semi-transparent dark purple shadow
    save_layer(img, char, "shadow")

    # 2. Body
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([390, 580, 634, 880], radius=80, fill=(76, 175, 80, 255)) # rich green body
    save_layer(img, char, "body")

    # 3. Head
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([340, 320, 684, 640], fill=(129, 199, 132, 255)) # lighter green head
    save_layer(img, char, "head")

    # 4. Left Arm
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([290, 600, 400, 690], fill=(76, 175, 80, 255))
    save_layer(img, char, "left_arm")

    # 5. Right Arm
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([624, 600, 734, 690], fill=(76, 175, 80, 255))
    save_layer(img, char, "right_arm")

    # 6. Hair (Double Leaf on Top)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    # Left Leaf
    draw.polygon([512, 330, 450, 220, 512, 250], fill=(56, 142, 60, 255))
    # Right Leaf
    draw.polygon([512, 330, 574, 220, 512, 250], fill=(56, 142, 60, 255))
    save_layer(img, char, "hair")

    # 7. Eyes Open
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    # Left Eye
    draw.ellipse([430, 440, 470, 490], fill=(24, 20, 38, 255))
    draw.ellipse([440, 445, 455, 465], fill=(255, 255, 255, 255)) # Eye shine
    # Right Eye
    draw.ellipse([554, 440, 594, 490], fill=(24, 20, 38, 255))
    draw.ellipse([564, 445, 579, 465], fill=(255, 255, 255, 255)) # Eye shine
    save_layer(img, char, "eyes_open")

    # 8. Eyes Closed (Happy arches)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    # Left Eye Arch
    draw.arc([430, 445, 470, 485], start=180, end=360, fill=(24, 20, 38, 255), width=8)
    # Right Eye Arch
    draw.arc([554, 445, 594, 485], start=180, end=360, fill=(24, 20, 38, 255), width=8)
    save_layer(img, char, "eyes_closed")

    # 9. Mouth
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([495, 505, 529, 535], fill=(255, 112, 67, 255)) # small coral open mouth
    save_layer(img, char, "mouth")

    # 9b. Mouth Happy
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.chord([485, 505, 539, 545], start=0, end=180, fill=(255, 112, 67, 255))
    save_layer(img, char, "mouth_happy")

    # 9c. Mouth Sad
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.arc([495, 505, 529, 535], start=180, end=360, fill=(24, 20, 38, 255), width=6)
    save_layer(img, char, "mouth_sad")


def generate_nimbus():
    char = "nimbus"
    
    # 1. Shadow
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([300, 900, 724, 970], fill=(24, 20, 38, 80))
    save_layer(img, char, "shadow")

    # 2. Body (Cloud puffs)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([370, 580, 550, 730], fill=(240, 248, 255, 255)) # Ice blue/white cloud puff
    draw.ellipse([470, 580, 650, 730], fill=(240, 248, 255, 255))
    draw.ellipse([420, 650, 600, 800], fill=(240, 248, 255, 255))
    save_layer(img, char, "body")

    # 3. Head (Large round fluffy cloud)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([320, 300, 704, 620], fill=(255, 255, 255, 255)) # Pure white head
    save_layer(img, char, "head")

    # 4. Left Arm (Cloud Paw)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([270, 600, 370, 700], fill=(240, 248, 255, 255))
    save_layer(img, char, "left_arm")

    # 5. Right Arm (Cloud Paw)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([654, 600, 754, 700], fill=(240, 248, 255, 255))
    save_layer(img, char, "right_arm")

    # 6. Hair (Fluffy top dog-ears)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([290, 270, 400, 420], fill=(240, 248, 255, 255)) # Left floppy cloud ear
    draw.ellipse([624, 270, 734, 420], fill=(240, 248, 255, 255)) # Right floppy cloud ear
    save_layer(img, char, "hair")

    # 7. Eyes Open
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([420, 420, 460, 470], fill=(24, 20, 38, 255))
    draw.ellipse([430, 425, 445, 445], fill=(255, 255, 255, 255))
    draw.ellipse([564, 420, 604, 470], fill=(24, 20, 38, 255))
    draw.ellipse([574, 425, 589, 445], fill=(255, 255, 255, 255))
    save_layer(img, char, "eyes_open")

    # 8. Eyes Closed
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.arc([420, 425, 460, 465], start=180, end=360, fill=(24, 20, 38, 255), width=8)
    draw.arc([564, 425, 604, 465], start=180, end=360, fill=(24, 20, 38, 255), width=8)
    save_layer(img, char, "eyes_closed")

    # 9. Mouth
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.arc([485, 465, 515, 495], start=0, end=180, fill=(24, 20, 38, 255), width=6) # double cat smile W
    draw.arc([509, 465, 539, 495], start=0, end=180, fill=(24, 20, 38, 255), width=6)
    save_layer(img, char, "mouth")

    # 9b. Mouth Happy
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.arc([485, 465, 515, 495], start=0, end=180, fill=(24, 20, 38, 255), width=6)
    draw.arc([509, 465, 539, 495], start=0, end=180, fill=(24, 20, 38, 255), width=6)
    draw.ellipse([497, 485, 527, 515], fill=(255, 138, 101, 255))
    save_layer(img, char, "mouth_happy")

    # 9c. Mouth Sad
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.arc([485, 485, 515, 515], start=180, end=360, fill=(24, 20, 38, 255), width=6)
    draw.arc([509, 485, 539, 515], start=180, end=360, fill=(24, 20, 38, 255), width=6)
    save_layer(img, char, "mouth_sad")


def generate_mocha():
    char = "mocha"
    
    # 1. Shadow
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([300, 900, 724, 970], fill=(24, 20, 38, 80))
    save_layer(img, char, "shadow")

    # 2. Body
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([380, 560, 644, 870], radius=90, fill=(141, 110, 99, 255)) # Teddy brown body
    draw.rounded_rectangle([440, 640, 584, 780], radius=50, fill=(215, 204, 200, 255)) # Cream belly patch
    save_layer(img, char, "body")

    # 3. Head
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([312, 280, 712, 610], fill=(141, 110, 99, 255)) # Teddy brown head
    # Left Ear
    draw.ellipse([300, 250, 410, 360], fill=(141, 110, 99, 255))
    draw.ellipse([325, 275, 385, 335], fill=(215, 204, 200, 255))
    # Right Ear
    draw.ellipse([614, 250, 724, 360], fill=(141, 110, 99, 255))
    draw.ellipse([639, 275, 699, 335], fill=(215, 204, 200, 255))
    save_layer(img, char, "head")

    # 4. Left Arm
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([290, 600, 390, 690], fill=(141, 110, 99, 255))
    save_layer(img, char, "left_arm")

    # 5. Right Arm
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([634, 600, 734, 690], fill=(141, 110, 99, 255))
    save_layer(img, char, "right_arm")

    # 6. Hair (Bear Top Tuft)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.polygon([512, 290, 495, 240, 512, 260], fill=(109, 76, 65, 255))
    draw.polygon([512, 290, 529, 240, 512, 260], fill=(109, 76, 65, 255))
    save_layer(img, char, "hair")

    # 7. Eyes Open
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    # Bear eyes (sleepy flat slits with brow)
    draw.rounded_rectangle([420, 420, 460, 435], radius=5, fill=(24, 20, 38, 255))
    draw.rounded_rectangle([564, 420, 604, 435], radius=5, fill=(24, 20, 38, 255))
    save_layer(img, char, "eyes_open")

    # 8. Eyes Closed
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    # Closed sleeping lines
    draw.line([415, 425, 465, 435], fill=(24, 20, 38, 255), width=8)
    draw.line([559, 435, 609, 425], fill=(24, 20, 38, 255), width=8)
    save_layer(img, char, "eyes_closed")

    # 9. Mouth
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    # Cream muzzle patch
    draw.ellipse([462, 440, 562, 510], fill=(245, 245, 245, 255))
    # Nose
    draw.ellipse([497, 450, 527, 470], fill=(24, 20, 38, 255))
    # Mouth line
    draw.line([512, 470, 512, 485], fill=(24, 20, 38, 255), width=4)
    draw.arc([497, 475, 512, 495], start=0, end=180, fill=(24, 20, 38, 255), width=4)
    draw.arc([512, 475, 527, 495], start=0, end=180, fill=(24, 20, 38, 255), width=4)
    save_layer(img, char, "mouth")

    # 9b. Mouth Happy
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([462, 440, 562, 510], fill=(245, 245, 245, 255))
    draw.ellipse([497, 450, 527, 470], fill=(24, 20, 38, 255))
    draw.line([512, 470, 512, 480], fill=(24, 20, 38, 255), width=4)
    draw.chord([482, 470, 542, 505], start=0, end=180, fill=(255, 112, 67, 255))
    save_layer(img, char, "mouth_happy")

    # 9c. Mouth Sad
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    draw.ellipse([462, 440, 562, 510], fill=(245, 245, 245, 255))
    draw.ellipse([497, 450, 527, 470], fill=(24, 20, 38, 255))
    draw.line([512, 470, 512, 480], fill=(24, 20, 38, 255), width=4)
    draw.arc([482, 480, 542, 515], start=180, end=360, fill=(24, 20, 38, 255), width=4)
    save_layer(img, char, "mouth_sad")

    # 10. Accessory (Cute pastel blue teacup)
    img = create_transparent_canvas()
    draw = ImageDraw.Draw(img)
    # Cup body
    draw.rounded_rectangle([470, 710, 554, 790], radius=15, fill=(129, 212, 250, 255))
    # Cup handle
    draw.arc([544, 725, 574, 775], start=270, end=90, fill=(129, 212, 250, 255), width=10)
    save_layer(img, char, "accessory")


if __name__ == "__main__":
    print("Generating Sprout layered sprites...")
    generate_sprout()
    print("Generating Nimbus layered sprites...")
    generate_nimbus()
    print("Generating Mocha layered sprites...")
    generate_mocha()
    print("All layers generated successfully!")
