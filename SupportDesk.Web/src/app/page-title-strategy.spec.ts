import { formatPageTitle } from './page-title-strategy';

describe('formatPageTitle', () => {
  it('prefixes the page title', () => {
    expect(formatPageTitle('Tickets')).toBe('SD: Tickets');
  });

  it('falls back to the app name when there is no title', () => {
    expect(formatPageTitle()).toBe('Support Desk');
  });
});
