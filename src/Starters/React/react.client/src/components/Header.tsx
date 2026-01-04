import { AppShell, Group, Burger } from '@mantine/core'
import { Link } from '@tanstack/react-router'

interface HeaderProps {
  opened: boolean
  toggle: () => void
}

export default function Header({ opened, toggle }: HeaderProps) {
  return (
    <AppShell.Header>
      <Group h="100%" px="md" justify="space-between">
        <Group>
          <Burger opened={opened} onClick={toggle} hiddenFrom="md" size="sm" />
          <Link to="/" style={{ textDecoration: 'none', color: 'inherit' }}>
            <Group gap="xs">
              Aufy Mantine
            </Group>
          </Link>
        </Group>
      </Group>
    </AppShell.Header>
  )
}

