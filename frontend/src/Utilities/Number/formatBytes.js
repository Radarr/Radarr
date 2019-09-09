import filesize from 'filesize';

function formatBytes(input, showBits = false) {
  const size = Number(input);

  if (isNaN(size)) {
    return '';
  }

  return filesize(size, {
    base: 2,
    round: 1,
    bits: showBits
  });
}

export default formatBytes;
