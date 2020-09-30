function isTomorrow(date) {
  if (!date) {
    return false;
  }

  const dateObj = (typeof date === 'object') ? date : new Date(date);
  const today = new Date();
  const tomorrow = new Date((today.setDate(today.getDate() + 1)));

  return dateObj.getDate() === tomorrow.getDate() && dateObj.getMonth() === tomorrow.getMonth() && dateObj.getFullYear() === tomorrow.getFullYear();
}

export default isTomorrow;
