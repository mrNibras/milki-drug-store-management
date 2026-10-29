import { BusinessClock } from './businessClock';

export function getTodayEthiopia(nowUtc: Date = new Date()): Date {
  // Use Ethiopia timezone
  const formatter = new Intl.DateTimeFormat('en-CA', {
    timeZone: 'Africa/Addis_Ababa',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  });
  const parts = formatter.formatToParts(nowUtc);
  const year = Number(parts.find(p => p.type === 'year')!.value);
  const month = Number(parts.find(p => p.type === 'month')!.value);
  const day = Number(parts.find(p => p.type === 'day')!.value);
  return new Date(Date.UTC(year, month - 1, day));
}

export function getCalendarDiff(from: Date, to: Date): { years: number; months: number; days: number } {
  const fromY = from.getUTCFullYear();
  const fromM = from.getUTCMonth();
  const fromD = from.getUTCDate();
  const toY = to.getUTCFullYear();
  const toM = to.getUTCMonth();
  const toD = to.getUTCDate();
  
  if (to < from) {
    const diff = getCalendarDiff(to, from);
    return { years: -diff.years, months: -diff.months, days: -diff.days };
  }
  
  let years = toY - fromY;
  let months = toM - fromM;
  let days = toD - fromD;
  
  if (days < 0) {
    months--;
    const previousMonth = new Date(Date.UTC(toY, toM, 0));
    days += previousMonth.getUTCDate();
  }
  
  if (months < 0) {
    years--;
    months += 12;
  }
  
  return { years, months, days };
}

export function formatExpiryCountdown(expiryDate: string, nowUtc: Date = new Date()): string {
  const expiry = new Date(expiryDate);
  const today = getTodayEthiopia(nowUtc);
  const expiryDateOnly = new Date(Date.UTC(expiry.getUTCFullYear(), expiry.getUTCMonth(), expiry.getUTCDate()));
  
  if (expiryDateOnly < today) {
    const diffMs = today.getTime() - expiryDateOnly.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    if (diffDays === 1) return 'Expired 1 day ago';
    if (diffDays === 0) return 'Expires today';
    return `Expired ${diffDays} days ago`;
  }
  
  if (expiryDateOnly.getTime() === today.getTime()) {
    return 'Expires today';
  }
  
  const remaining = getCalendarDiff(today, expiryDateOnly);
  const { years, months, days } = remaining;
  
  if (years === 0 && months === 0) {
    if (days === 1) return '1 day left for expiry';
    return `${days} days left for expiry`;
  }
  
  const parts: string[] = [];
  if (years > 0) parts.push(years === 1 ? '1 year' : `${years} years`);
  if (months > 0) parts.push(months === 1 ? '1 month' : `${months} months`);
  if (days > 0) parts.push(days === 1 ? '1 day' : `${days} days`);
  
  const result = parts.join(' and ');
  return `${result} left for expiry`;
}
