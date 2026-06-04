import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import LoginForm from '../components/LoginForm';

describe('LoginForm', () => {
  beforeEach(() => {
    render(
      <MemoryRouter>
        <LoginForm selectedRole="Admin" />
      </MemoryRouter>
    );
  });

  it('renders the Username input with helper placeholder', () => {
    expect(screen.getByLabelText('Username')).toBeTruthy();
    expect(screen.getByPlaceholderText('Email or Employee ID')).toBeTruthy();
  });

  it('renders the Password input and password visibility toggle button', () => {
    expect(screen.getByLabelText('Password')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Show password' })).toBeTruthy();
  });

  it('renders keep-me-logged-in and forgot-password controls', () => {
    expect(screen.getByRole('checkbox', { name: 'Keep me logged in' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Forgot Password' })).toBeTruthy();
  });

  it('renders login action and sign-up link', () => {
    expect(screen.getByRole('button', { name: 'Login' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Sign up' })).toBeTruthy();
  });

  it('toggles password visibility text and input type', async () => {
    const user = userEvent.setup();
    const passwordInput = screen.getByLabelText('Password') as HTMLInputElement;

    expect(passwordInput.type).toBe('password');

    await user.click(screen.getByRole('button', { name: 'Show password' }));

    expect(passwordInput.type).toBe('text');
    expect(screen.getByRole('button', { name: 'Hide password' })).toBeTruthy();
  });
});
