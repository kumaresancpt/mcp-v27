export type UserRole = 'Admin' | 'Receptionist' | 'Security Guard';

type RoleSelectorProps = {
  selectedRole: UserRole;
  onRoleChange: (role: UserRole) => void;
};

const roles: UserRole[] = ['Admin', 'Receptionist', 'Security Guard'];

export default function RoleSelector({
  selectedRole,
  onRoleChange
}: RoleSelectorProps): JSX.Element {
  return (
    <div
      role="tablist"
      aria-label="Login role selector"
      style={{
        width: '100%',
        height: '56px',
        borderRadius: '48px',
        backgroundColor: '#F3F3F3',
        display: 'flex',
        gap: '8px',
        padding: '4px',
        boxSizing: 'border-box'
      }}
    >
      {roles.map((role) => {
        const active = selectedRole === role;

        return (
          <button
            key={role}
            type="button"
            role="tab"
            aria-selected={active}
            onClick={() => onRoleChange(role)}
            style={{
              flex: 1,
              border: 'none',
              borderRadius: '44px',
              backgroundColor: active ? '#5B21B6' : 'transparent',
              color: active ? '#FFFFFF' : '#353638',
              fontSize: '14px',
              fontWeight: active ? 700 : 600,
              lineHeight: '20px',
              cursor: 'pointer'
            }}
          >
            {role}
          </button>
        );
      })}
    </div>
  );
}
