import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuthNavigation } from '../hooks/useAuthNavigation';
import { type UserRole } from './RoleSelector';

type LoginFormProps = {
  selectedRole: UserRole;
};

type LoginResponse = {
  accessToken?: string;
  expiresAtUtc?: string;
  role?: string;
  userId?: string;
  sessionId?: string;
  detail?: string;
};

const supportedRoles: UserRole[] = ['Admin', 'Receptionist', 'Security Guard'];

function getAuthenticatedRole(role: string | undefined, fallbackRole: UserRole): UserRole {
  if (role && supportedRoles.includes(role as UserRole)) {
    return role as UserRole;
  }

  return fallbackRole;
}

export default function LoginForm({ selectedRole }: LoginFormProps): JSX.Element {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [keepMeLoggedIn, setKeepMeLoggedIn] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { navigateToRoleDashboard } = useAuthNavigation();

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setApiError(null);
    setSuccessMessage(null);

    if (!username.trim() || !password.trim()) {
      setApiError('Username and password are required.');
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify({
          username,
          password,
          role: selectedRole,
          keepMeLoggedIn
        })
      });

      let data: LoginResponse | null = null;

      try {
        data = (await response.json()) as LoginResponse;
      } catch {
        data = null;
      }

      if (!response.ok) {
        setApiError(data?.detail ?? 'Invalid credentials. Please try again.');
        return;
      }

      if (!data?.accessToken) {
        setApiError('Authentication response was incomplete. Please try again.');
        return;
      }

      const authenticatedRole = getAuthenticatedRole(data.role, selectedRole);

      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('userRole', authenticatedRole);

      if (data.expiresAtUtc) {
        localStorage.setItem('expiresAtUtc', data.expiresAtUtc);
      }

      if (data.sessionId) {
        localStorage.setItem('sessionId', data.sessionId);
      }

      if (data.userId) {
        localStorage.setItem('userId', data.userId);
      }

      setSuccessMessage('Login successful. Redirecting...');
      navigateToRoleDashboard(authenticatedRole);
    } catch {
      setApiError('Authentication service is unavailable. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
      {apiError && (
        <div
          role="alert"
          style={{
            border: '1px solid #DC2626',
            borderRadius: '8px',
            backgroundColor: '#FEE2E2',
            color: '#991B1B',
            fontSize: '14px',
            padding: '10px 12px'
          }}
        >
          {apiError}
        </div>
      )}

      {successMessage && (
        <div
          role="status"
          style={{
            border: '1px solid #16A34A',
            borderRadius: '8px',
            backgroundColor: '#DCFCE7',
            color: '#166534',
            fontSize: '14px',
            padding: '10px 12px'
          }}
        >
          {successMessage}
        </div>
      )}

      <label
        htmlFor="username"
        style={{ display: 'flex', flexDirection: 'column', gap: '8px', color: '#353638', fontWeight: 600 }}
      >
        Username
        <input
          id="username"
          name="username"
          type="text"
          value={username}
          onChange={(event) => setUsername(event.target.value)}
          placeholder="Email or Employee ID"
          autoComplete="username"
          style={{
            width: '100%',
            height: '48px',
            borderRadius: '8px',
            border: '1px solid #B9B9B9',
            padding: '0 14px',
            boxSizing: 'border-box',
            fontSize: '14px',
            color: '#353638'
          }}
        />
      </label>

      <label
        htmlFor="password"
        style={{ display: 'flex', flexDirection: 'column', gap: '8px', color: '#353638', fontWeight: 600 }}
      >
        Password
        <div style={{ position: 'relative' }}>
          <input
            id="password"
            name="password"
            type={showPassword ? 'text' : 'password'}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="Enter password"
            autoComplete="current-password"
            style={{
              width: '100%',
              height: '48px',
              borderRadius: '8px',
              border: '1px solid #B9B9B9',
              padding: '0 44px 0 14px',
              boxSizing: 'border-box',
              fontSize: '14px',
              color: '#353638'
            }}
          />
          <button
            type="button"
            aria-label={showPassword ? 'Hide password' : 'Show password'}
            onClick={() => setShowPassword((value) => !value)}
            style={{
              position: 'absolute',
              right: '12px',
              top: '50%',
              transform: 'translateY(-50%)',
              border: 'none',
              background: 'transparent',
              color: '#5B21B6',
              cursor: 'pointer',
              fontSize: '12px',
              fontWeight: 700
            }}
          >
            {showPassword ? 'Hide' : 'Show'}
          </button>
        </div>
      </label>

      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '10px' }}>
        <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '14px', color: '#353638' }}>
          <input
            type="checkbox"
            checked={keepMeLoggedIn}
            onChange={(event) => setKeepMeLoggedIn(event.target.checked)}
          />
          Keep me logged in
        </label>

        <a
          href="/forgot-password"
          style={{ color: '#5B21B6', fontWeight: 600, fontSize: '14px', textDecoration: 'none' }}
        >
          Forgot Password
        </a>
      </div>

      <button
        type="submit"
        disabled={isSubmitting}
        style={{
          width: '100%',
          height: '48px',
          border: 'none',
          borderRadius: '8px',
          backgroundColor: '#5B21B6',
          color: '#FFFFFF',
          fontSize: '16px',
          fontWeight: 700,
          cursor: isSubmitting ? 'not-allowed' : 'pointer',
          opacity: isSubmitting ? 0.7 : 1
        }}
      >
        {isSubmitting ? 'Logging in...' : 'Login'}
      </button>

      <p style={{ margin: 0, textAlign: 'center', fontSize: '14px', color: '#353638' }}>
        Don&apos;t have an account?{' '}
        <Link to="/signup" style={{ color: '#5B21B6', fontWeight: 700, textDecoration: 'none' }}>
          Sign up
        </Link>
      </p>
    </form>
  );
}
