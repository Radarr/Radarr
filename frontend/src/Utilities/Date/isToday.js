function isToday(date) {
  if (!date) {
    return false;
  }

  const dateObj = (typeof date === 'object') ? date : new Date(date);
  const today = new Date();

  return dateObj.getDate() === today.getDate() && dateObj.getMonth() === today.getMonth() && dateObj.getFullYear() === today.getFullYear();
}

export default isToday;
