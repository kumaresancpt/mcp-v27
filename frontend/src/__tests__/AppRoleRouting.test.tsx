import { render, screen } from '@testing-library/react';
import App from '../App';

describe('App role routing', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('redirects root path to login page', async () => {
    window.history.pushState({}, '', '/');

    render(<App />);

    expect(await screen.findByRole('heading', { name: 'Login' })).toBeTruthy();
  });

  it('redirects protected admin path to login when token is missing', async () => {
    window.history.pushState({}, '', '/admin');

    render(<App />);

    expect(await screen.findByRole('heading', { name: 'Login' })).toBeTruthy();
  });

  it('renders Admin Dashboard when token exists and route is /admin', async () => {
    localStorage.setItem('accessToken', 'token-admin');
    window.history.pushState({}, '', '/admin');

    render(<App />);

    expect(await screen.findByRole('heading', { name: 'Admin Dashboard' })).toBeTruthy();
  });

  it('renders Receptionist Dashboard when token exists and route is /receptionist', async () => {
    localStorage.setItem('accessToken', 'token-receptionist');
    window.history.pushState({}, '', '/receptionist');

    render(<App />);

    expect(await screen.findByRole('heading', { name: 'Receptionist Dashboard' })).toBeTruthy();
  });

  it('renders Security Guard Dashboard when token exists and route is /security', async () => {
    localStorage.setItem('accessToken', 'token-security');
    window.history.pushState({}, '', '/security');

    render(<App />);

    expect(await screen.findByRole('heading', { name: 'Security Guard Dashboard' })).toBeTruthy();
  });
});
