import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import LoginPage from '../pages/LoginPage';

describe('LoginPage', () => {
  beforeEach(() => {
    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );
  });

  it('renders the login hero region', () => {
    expect(screen.getByLabelText('Login hero')).toBeTruthy();
  });

  it('renders the login form panel region', () => {
    expect(screen.getByLabelText('Login form panel')).toBeTruthy();
  });

  it('renders the Login heading text', () => {
    expect(screen.getByRole('heading', { name: 'Login' })).toBeTruthy();
  });

  it('renders the welcome copy', () => {
    expect(screen.getByText('Welcome to Visitor')).toBeTruthy();
  });

  it('renders the hero image with descriptive alt text', () => {
    expect(screen.getByAltText('Visitors meeting at the gated entrance')).toBeTruthy();
  });
});
