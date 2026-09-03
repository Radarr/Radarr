// Parses a runtime string in "H:MM:SS" or "M:SS" format to decimal hours
function parseRuntimeToHours(runTime: string | undefined): number {
  if (!runTime) {
    return 0;
  }

  const parts = runTime.split(':').map(Number);

  if (parts.some(isNaN)) {
    return 0;
  }

  if (parts.length === 3) {
    // H:MM:SS format
    return parts[0] + parts[1] / 60 + parts[2] / 3600;
  }

  if (parts.length === 2) {
    // M:SS format
    return parts[0] / 60 + parts[1] / 3600;
  }

  return 0;
}

export default parseRuntimeToHours;
