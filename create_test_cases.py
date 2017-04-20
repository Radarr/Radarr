input1 = """Prometheus.Special.Edition.Fan Edit.2012..BRRip.x264.AAC-m2g
Star Wars Episode IV - A New Hope (Despecialized) 1999.mkv
Prometheus.(Special.Edition.Remastered).2012.[Bluray-1080p].mkv
Prometheus Extended 2012
Prometheus Extended Directors Cut Fan Edit 2012
Prometheus Director's Cut 2012
Prometheus Directors Cut 2012
Prometheus.(Extended.Theatrical.Version.IMAX).BluRay.1080p.2012.asdf
2001 A Space Odyssey Director's Cut (1968).mkv
2001: A Space Odyssey (Extended Directors Cut FanEdit) Bluray 1080p 1968
A Fake Movie 2035 Directors 2012.mkv
Blade Runner Director's Cut 2049.mkv
Prometheus 50th Anniversary Edition 2012.mkv
Movie 2in1 2012.mkv
Movie IMAX 2012.mkv"""

output1 = """Special.Edition.Fan Edit     BRRip.x264.AAC-m2g
Despecialized     mkv
Special.Edition.Remastered     Bluray-1080p].mkv
Extended     mkv
Extended Directors Cut Fan Edit     mkv
Director's Cut     mkv
Directors Cut     mkv
Extended.Theatrical.Version.IMAX     asdf
Director's Cut     mkv
Extended Directors Cut FanEdit     mkv
Directors     mkv
Director's Cut     mkv
50th Anniversary Edition     mkv
2in1     mkv
IMAX     mkv"""

inputs = input1.split("\n")
outputs = output1.split("\n")
real_o = []
for output in outputs:
    real_o.append(output.split("   ")[0].replace(".", " ").strip())

count = 0

for inp in inputs:
    o = real_o[count]
    print "[TestCase(\"{0}\", \"{1}\")]".format(inp, o)
    count += 1
