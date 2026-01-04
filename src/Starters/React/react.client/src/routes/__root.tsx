import { Outlet, createRootRoute } from '@tanstack/react-router'
import { TanStackRouterDevtoolsPanel } from '@tanstack/react-router-devtools'
import { TanStackDevtools } from '@tanstack/react-devtools'
import { AppShell, Drawer } from '@mantine/core'
import { useDisclosure, useMediaQuery } from '@mantine/hooks'
import Header from '@/components/Header'
import Navigation from '@/components/Navigation'

export const Route = createRootRoute({
  component: () => {
    const [opened, { toggle, close }] = useDisclosure(false)
    const isMobile = useMediaQuery('(max-width: 768px)')

    return (
      <>
        <AppShell
          header={{ height: 60 }}
          navbar={{
            width: 300,
            breakpoint: 'md',
            collapsed: { mobile: true, desktop: false },
          }}
          padding="md"
        >
          <Header opened={opened} toggle={toggle} />

          {!isMobile && (
            <AppShell.Navbar p="md">
              <Navigation />
            </AppShell.Navbar>
          )}

          <AppShell.Main>
            <Outlet />
          </AppShell.Main>

          <TanStackDevtools
            config={{
              position: 'bottom-right',
            }}
            plugins={[
              {
                name: 'Tanstack Router',
                render: <TanStackRouterDevtoolsPanel />,
              },
            ]}
          />
        </AppShell>

        <Drawer
          opened={opened}
          onClose={close}
          title="Navigation"
          position="left"
          hiddenFrom="md"
          padding="md"
        >
          <Navigation onNavigate={isMobile ? close : undefined} />
        </Drawer>
      </>
    )
  },
})
