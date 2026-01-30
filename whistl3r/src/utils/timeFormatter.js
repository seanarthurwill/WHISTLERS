/**
 * Formats a TimeSpan string (HH:MM:SS) to 12-hour format (hh:mm AM/PM)
 * @param {string} timeSpan - Time in format "HH:MM:SS" or "HH:MM:SS.ffffff"
 * @returns {string} Formatted time in 12-hour format
 */
export function formatTime12Hour(timeSpan) {
  if (!timeSpan) return '';
  
  // Parse the time string (format: "HH:MM:SS" or "HH:MM:SS.ffffff")
  const timeParts = timeSpan.split(':');
  if (timeParts.length < 2) return timeSpan;
  
  let hours = parseInt(timeParts[0], 10);
  const minutes = timeParts[1];
  
  // Determine AM/PM
  const period = hours >= 12 ? 'PM' : 'AM';
  
  // Convert to 12-hour format
  if (hours === 0) {
    hours = 12; // Midnight
  } else if (hours > 12) {
    hours = hours - 12;
  }
  
  return `${hours}:${minutes} ${period}`;
}
