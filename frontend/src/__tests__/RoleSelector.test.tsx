import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import RoleSelector from '../components/RoleSelector';

describe('RoleSelector', () => {
  it('renders the role selector tablist', () => {
    const onRoleChange = jest.fn();

    render(
      <MemoryRouter>
        <RoleSelector selectedRole="Admin" onRoleChange={onRoleChange} />
      </MemoryRouter>
    );

    expect(screen.getByRole('tablist', { name: 'Login role selector' })).toBeTruthy();
  });

  it('renders Admin, Receptionist, and Security Guard tabs', () => {
    const onRoleChange = jest.fn();

    render(
      <MemoryRouter>
        <RoleSelector selectedRole="Admin" onRoleChange={onRoleChange} />
      </MemoryRouter>
    );

    expect(screen.getByRole('tab', { name: 'Admin' })).toBeTruthy();
    expect(screen.getByRole('tab', { name: 'Receptionist' })).toBeTruthy();
    expect(screen.getByRole('tab', { name: 'Security Guard' })).toBeTruthy();
  });

  it('marks only the selected role as active', () => {
    const onRoleChange = jest.fn();

    render(
      <MemoryRouter>
        <RoleSelector selectedRole="Admin" onRoleChange={onRoleChange} />
      </MemoryRouter>
    );

    expect(screen.getByRole('tab', { name: 'Admin' }).getAttribute('aria-selected')).toBe('true');
    expect(screen.getByRole('tab', { name: 'Receptionist' }).getAttribute('aria-selected')).toBe('false');
    expect(screen.getByRole('tab', { name: 'Security Guard' }).getAttribute('aria-selected')).toBe('false');
  });

  it('calls onRoleChange when Receptionist is clicked', async () => {
    const user = userEvent.setup();
    const onRoleChange = jest.fn();

    render(
      <MemoryRouter>
        <RoleSelector selectedRole="Admin" onRoleChange={onRoleChange} />
      </MemoryRouter>
    );

    await user.click(screen.getByRole('tab', { name: 'Receptionist' }));

    expect(onRoleChange).toHaveBeenCalledWith('Receptionist');
  });

  it('calls onRoleChange when Security Guard is clicked', async () => {
    const user = userEvent.setup();
    const onRoleChange = jest.fn();

    render(
      <MemoryRouter>
        <RoleSelector selectedRole="Admin" onRoleChange={onRoleChange} />
      </MemoryRouter>
    );

    await user.click(screen.getByRole('tab', { name: 'Security Guard' }));

    expect(onRoleChange).toHaveBeenCalledWith('Security Guard');
  });
});
