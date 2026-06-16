import { useNavigate } from 'react-router-dom';
import { type UserRole } from '../components/RoleSelector';

function getRoleRoute(role: UserRole): string {
  if (role === 'Admin') {
    return '/admin';
  }

  if (role === 'Receptionist') {
    return '/receptionist';
  }

  return '/security';
}

export function useAuthNavigation(): {
  navigateToRoleDashboard: (role: UserRole) => void;
} {
  const navigate = useNavigate();

  function navigateToRoleDashboard(role: UserRole): void {
    navigate(getRoleRoute(role), { replace: true });
  }

  return { navigateToRoleDashboard };
}
