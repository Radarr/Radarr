// This is the output of eg. http://192.168.0.3:8089/conversion/book-data/100

const profileData = {
  cybookg3: {
    name: 'Cybook G3',
    description: 'This profile is intended for the Cybook G3. [Screen size: 600 x 800 pixels]'
  },
  cybook_opus: {
    name: 'Cybook Opus',
    description: 'This profile is intended for the Cybook Opus. [Screen size: 590 x 775 pixels]'
  },
  default: {
    name: 'Default Output Profile',
    description: 'This profile tries to provide sane defaults and is useful if you want to produce a document intended to be read at a computer or on a range of devices. [Screen size: 1600 x 1200 pixels]'
  },
  generic_eink: {
    name: 'Generic e-ink',
    description: 'Suitable for use with any e-ink device [Screen size: 590 x 775 pixels]'
  },
  generic_eink_hd: {
    name: 'Generic e-ink HD',
    description: 'Suitable for use with any modern high resolution e-ink device [Screen size: unlimited]'
  },
  generic_eink_large: {
    name: 'Generic e-ink large',
    description: 'Suitable for use with any large screen e-ink device [Screen size: 600 x 999 pixels]'
  },
  hanlinv3: {
    name: 'Hanlin V3',
    description: 'This profile is intended for the Hanlin V3 and its clones. [Screen size: 584 x 754 pixels]'
  },
  hanlinv5: {
    name: 'Hanlin V5',
    description: 'This profile is intended for the Hanlin V5 and its clones. [Screen size: 584 x 754 pixels]'
  },
  illiad: {
    name: 'Illiad',
    description: 'This profile is intended for the Irex Illiad. [Screen size: 760 x 925 pixels]'
  },
  ipad: {
    name: 'iPad',
    description: 'Intended for the iPad and similar devices with a resolution of 768x1024 [Screen size: 768 x 1024 pixels]'
  },
  ipad3: {
    name: 'iPad 3',
    description: 'Intended for the iPad 3 and similar devices with a resolution of 1536x2048 [Screen size: 2048 x 1536 pixels]'
  },
  irexdr1000: {
    name: 'IRex Digital Reader 1000',
    description: 'This profile is intended for the IRex Digital Reader 1000. [Screen size: 1024 x 1280 pixels]'
  },
  irexdr800: {
    name: 'IRex Digital Reader 800',
    description: 'This profile is intended for the IRex Digital Reader 800. [Screen size: 768 x 1024 pixels]'
  },
  jetbook5: {
    name: 'JetBook 5-inch',
    description: 'This profile is intended for the 5-inch JetBook. [Screen size: 480 x 640 pixels]'
  },
  kindle: {
    name: 'Kindle',
    description: 'This profile is intended for the Amazon Kindle. [Screen size: 525 x 640 pixels]'
  },
  kindle_dx: {
    name: 'Kindle DX',
    description: 'This profile is intended for the Amazon Kindle DX. [Screen size: 744 x 1022 pixels]'
  },
  kindle_fire: {
    name: 'Kindle Fire',
    description: 'This profile is intended for the Amazon Kindle Fire. [Screen size: 570 x 1016 pixels]'
  },
  kindle_oasis: {
    name: 'Kindle Oasis',
    description: 'This profile is intended for the Amazon Kindle Oasis 2017 and above [Screen size: 1264 x 1680 pixels]'
  },
  kindle_pw: {
    name: 'Kindle PaperWhite',
    description: 'This profile is intended for the Amazon Kindle PaperWhite 1 and 2 [Screen size: 658 x 940 pixels]'
  },
  kindle_pw3: {
    name: 'Kindle PaperWhite 3',
    description: 'This profile is intended for the Amazon Kindle PaperWhite 3 and above [Screen size: 1072 x 1430 pixels]'
  },
  kindle_voyage: {
    name: 'Kindle Voyage',
    description: 'This profile is intended for the Amazon Kindle Voyage [Screen size: 1080 x 1430 pixels]'
  },
  kobo: {
    name: 'Kobo Reader',
    description: 'This profile is intended for the Kobo Reader. [Screen size: 536 x 710 pixels]'
  },
  msreader: {
    name: 'Microsoft Reader',
    description: 'This profile is intended for the Microsoft Reader. [Screen size: 480 x 652 pixels]'
  },
  mobipocket: {
    name: 'Mobipocket Books',
    description: 'This profile is intended for the Mobipocket books. [Screen size: 600 x 800 pixels]'
  },
  nook: {
    name: 'Nook',
    description: 'This profile is intended for the B&N Nook. [Screen size: 600 x 730 pixels]'
  },
  nook_color: {
    name: 'Nook Color',
    description: 'This profile is intended for the B&N Nook Color. [Screen size: 600 x 900 pixels]'
  },
  nook_hd_plus: {
    name: 'Nook HD+',
    description: 'Intended for the Nook HD+ and similar tablet devices with a resolution of 1280x1920 [Screen size: 1280 x 1920 pixels]'
  },
  pocketbook_900: {
    name: 'PocketBook Pro 900',
    description: 'This profile is intended for the PocketBook Pro 900 series of devices. [Screen size: 810 x 1180 pixels]'
  },
  pocketbook_pro_912: {
    name: 'PocketBook Pro 912',
    description: 'This profile is intended for the PocketBook Pro 912 series of devices. [Screen size: 825 x 1200 pixels]'
  },
  galaxy: {
    name: 'Samsung Galaxy',
    description: 'Intended for the Samsung Galaxy and similar tablet devices with a resolution of 600x1280 [Screen size: 600 x 1280 pixels]'
  },
  sony: {
    name: 'Sony Reader',
    description: 'This profile is intended for the SONY PRS line. The 500/505/600/700 etc. [Screen size: 590 x 775 pixels]'
  },
  sony300: {
    name: 'Sony Reader 300',
    description: 'This profile is intended for the SONY PRS-300. [Screen size: 590 x 775 pixels]'
  },
  sony900: {
    name: 'Sony Reader 900',
    description: 'This profile is intended for the SONY PRS-900. [Screen size: 600 x 999 pixels]'
  },
  sony_landscape: {
    name: 'Sony Reader Landscape',
    description: 'This profile is intended for the SONY PRS line. The 500/505/700 etc, in landscape mode. Mainly useful for comics. [Screen size: 784 x 1012 pixels]'
  },
  sonyt3: {
    name: 'Sony Reader T3',
    description: 'This profile is intended for the SONY PRS-T3. [Screen size: 758 x 934 pixels]'
  },
  tablet: {
    name: 'Tablet',
    description: 'Intended for generic tablet devices, does no resizing of images [Screen size: unlimited]'
  }
};

export const options = Object.entries(profileData).map(([key, object]) => {
  return {
    key,
    value: object.name,
    description: object.description
  };
});
