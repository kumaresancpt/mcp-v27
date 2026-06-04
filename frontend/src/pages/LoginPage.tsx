import { useState } from 'react';
import LoginForm from '../components/LoginForm';
import RoleSelector, { type UserRole } from '../components/RoleSelector';

const heroImageUrl = 'https://www.figma.com/api/mcp/asset/a4ffe847-0949-4a16-8846-f137856438b7';
const visitorLogoUrl = 'https://www.figma.com/api/mcp/asset/f4b34f69-d77b-4f26-9fbe-137f533eef05';
const changepondLogoUrl = 'https://www.figma.com/api/mcp/asset/ac5ae9de-8bcd-4f24-8e73-093cb43c8fc1';

export default function LoginPage(): JSX.Element {
  const [selectedRole, setSelectedRole] = useState<UserRole>('Admin');

  return (
    <main
      style={{
        width: '100%',
        minHeight: '100vh',
        display: 'flex',
        backgroundColor: '#F3F3F3',
        fontFamily: 'Inter, sans-serif'
      }}
    >
      <section
        aria-label="Login hero"
        style={{
          flex: 1,
          minHeight: '100vh',
          position: 'relative',
          overflow: 'hidden',
          backgroundColor: '#EDE8E1'
        }}
      >
        <img
          src={heroImageUrl}
          alt="Visitors meeting at the gated entrance"
          style={{
            position: 'absolute',
            inset: 0,
            width: '100%',
            height: '100%',
            objectFit: 'cover',
            objectPosition: 'center'
          }}
        />
        <div
          style={{
            position: 'absolute',
            inset: '0',
            background:
              'linear-gradient(180deg, rgba(255,255,255,0.08) 0%, rgba(53,54,56,0.06) 100%)'
          }}
        />
      </section>

      <section
        aria-label="Login form panel"
        style={{
          width: '596px',
          minHeight: '100vh',
          borderTopLeftRadius: '56px',
          borderBottomLeftRadius: '56px',
          backgroundColor: '#FFFFFF',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          padding: '40px 48px 24px 48px',
          boxSizing: 'border-box'
        }}
      >
        <div style={{ display: 'flex', flexDirection: 'column', gap: '32px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '10px'
              }}
            >
              <img
                src={visitorLogoUrl}
                alt="Visitor logo"
                style={{ width: '46px', height: '46px', objectFit: 'contain' }}
              />
              <div style={{ display: 'flex', flexDirection: 'column', gap: '2px' }}>
                <div
                  style={{
                    fontFamily: 'Satoshi, Inter, sans-serif',
                    fontSize: '26px',
                    fontWeight: 700,
                    color: '#5B21B6',
                    lineHeight: 1
                  }}
                >
                  VISITOR
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                  <span
                    style={{
                      fontSize: '11px',
                      fontWeight: 500,
                      color: '#353638',
                      opacity: 0.7
                    }}
                  >
                    Powered by
                  </span>
                  <img
                    src={changepondLogoUrl}
                    alt="Changepond"
                    style={{ height: '13px', objectFit: 'contain' }}
                  />
                </div>
              </div>
            </div>
            <div />
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
            <h1
              style={{
                margin: 0,
                fontFamily: 'Satoshi, Inter, sans-serif',
                fontSize: '40px',
                lineHeight: '48px',
                fontWeight: 700,
                color: '#353638'
              }}
            >
              Login
            </h1>
            <p
              style={{
                margin: 0,
                fontSize: '16px',
                lineHeight: '24px',
                fontWeight: 500,
                color: '#353638',
                opacity: 0.8
              }}
            >
              Welcome to Visitor
            </p>
          </div>

          <RoleSelector selectedRole={selectedRole} onRoleChange={setSelectedRole} />
          <LoginForm selectedRole={selectedRole} />
        </div>

        <footer
          style={{
            fontSize: '12px',
            lineHeight: '16px',
            color: '#353638',
            opacity: 0.7,
            textAlign: 'center'
          }}
        >
          Copyright 2026 Changepond. All Rights Reserved.
        </footer>
      </section>
    </main>
  );
}
