file1_path = "C:/Users/steph/Documents/CAB401/DigitalMusicAnalysis_Sequential/DigitalMusicAnalysis/sequentialOutput.txt"
file2_path = "C:/Users/steph/Documents/CAB401/DigitalMusicAnalysis/DigitalMusicAnalysis/ParallleOutput.txt"

with open(file1_path, "r") as file1, open(file2_path, "r") as file2:
    lines1 = file1.readlines()
    lines2 = file2.readlines()

lines_match = True



# Compare line by line
for i, (line1, line2) in enumerate(zip(lines1, lines2)):
    if line1 != line2:
        print(f"Difference in line {i + 1}:")
        print(f"File 1: {line1.strip()}")
        print(f"File 2: {line2.strip()}")
        lines_match = False



if lines_match:
    print ("All strings are matched")

    