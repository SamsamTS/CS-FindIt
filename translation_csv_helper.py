# Just a simple python3 script
# It processes downloaded google sheet translation csv to xml readable by the mod

import sys, csv

# column of key
col_key = 0
# column of value
col_value = 7

csv_to_open = sys.argv[1]
xml_to_save = csv_to_open.split('.')[0] + ".xml"

ofile = open(xml_to_save, "w+")
ofile.write(f"<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n")

unique_name = ""
readable_name = ""

with open(csv_to_open) as csvfile:
    reader = csv.reader(csvfile, delimiter=',')
    counter = 0
    for row in reader:
        if counter == 1:
            unique_name = row[col_value]
        elif counter == 2:
            readable_name = row[col_value]
            ofile.write(f"<Language UniqueName=\"{unique_name}\" ReadableName=\"{readable_name}\">\n")
            ofile.write(f"  <Translations>\n")
        elif counter > 2:
            if row[col_value] == "": continue
            ofile.write(f"    <Translation ID=\"{row[col_key]}\" String=\"{row[col_value]}\" />\n")


        counter = counter + 1

ofile.write(f"\n  </Translations>\n")
ofile.write("</Language>\n")

ofile.close()
