function isYesterday(date) {
  if (!date) {
    return false;
  }

  const dateObj = (typeof date === 'object') ? date : new Date(date);
  const today = new Date();
  const yesterday = new Date((today.setDate(today.getDate() - 1)));

  return dateObj.getDate() === yesterday.getDate() && dateObj.getMonth() === yesterday.getMonth() && dateObj.getFullYear() === yesterday.getFullYear();
}

export default isYesterday;
