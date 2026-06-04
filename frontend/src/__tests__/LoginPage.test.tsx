import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from '../App';

describe('login flow', () => {
  const originalFetch = globalThis.fetch;

  beforeEach(() => {
    window.history.pushState({}, '', '/login');
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
    jest.resetAllMocks();

    if (originalFetch) {
      globalThis.fetch = originalFetch;
      return;
    }

    delete (globalThis as Partial<typeof globalThis>).fetch;
  });

  it('shows a validation error when credentials are missing', async () => {
    const user = userEvent.setup();

    render(<App />);

    await user.click(screen.getByRole('button', { name: /^login$/i }));

    expect((await screen.findByRole('alert')).textContent).toContain('Username and password are required.');
  });

  it('submits the selected role and routes to the matching dashboard', async () => {
    const fetchMock = jest.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        accessToken: 'token-123',
        role: 'Receptionist',
        expiresAtUtc: '2026-06-05T00:00:00Z',
        sessionId: 'session-1',
        userId: 'user-1'
      })
    });
    const user = userEvent.setup();

    globalThis.fetch = fetchMock as unknown as typeof fetch;

    render(<App />);

    await user.click(screen.getByRole('tab', { name: 'Receptionist' }));
    await user.type(screen.getByLabelText(/username/i), 'recep.user');
    await user.type(screen.getByLabelText(/^password$/i), 'SecurePass123');
    await user.click(screen.getByRole('checkbox', { name: /keep me logged in/i }));
    await user.click(screen.getByRole('button', { name: /^login$/i }));

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));

  const requestInit = fetchMock.mock.calls[0]?.[1] as RequestInit | undefined;
    const requestBody = JSON.parse(String(requestInit?.body));

    expect(requestInit).toMatchObject({
      method: 'POST',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json'
      }
    });
    expect(requestBody).toEqual({
      username: 'recep.user',
      password: 'SecurePass123',
      role: 'Receptionist',
      keepMeLoggedIn: true
    });
    expect(await screen.findByRole('heading', { name: 'Receptionist Dashboard' })).toBeTruthy();
    expect(localStorage.getItem('accessToken')).toBe('token-123');
    expect(localStorage.getItem('userRole')).toBe('Receptionist');
    expect(localStorage.getItem('expiresAtUtc')).toBe('2026-06-05T00:00:00Z');
    expect(localStorage.getItem('sessionId')).toBe('session-1');
    expect(localStorage.getItem('userId')).toBe('user-1');
  });
});
