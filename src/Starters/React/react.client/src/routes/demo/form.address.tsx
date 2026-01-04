import { createFileRoute } from '@tanstack/react-router'
import { Container, Paper, Stack, Group, Grid } from '@mantine/core'

import { useAppForm } from '@/hooks/demo.form'

export const Route = createFileRoute('/demo/form/address')({
  component: AddressForm,
})

function AddressForm() {
  const form = useAppForm({
    defaultValues: {
      fullName: '',
      email: '',
      address: {
        street: '',
        city: '',
        state: '',
        zipCode: '',
        country: '',
      },
      phone: '',
    },
    validators: {
      onBlur: ({ value }) => {
        const errors = {
          fields: {},
        } as {
          fields: Record<string, string>
        }
        if (value.fullName.trim().length === 0) {
          errors.fields.fullName = 'Full name is required'
        }
        return errors
      },
    },
    onSubmit: ({ value }) => {
      console.log(value)
      // Show success message
      alert('Form submitted successfully!')
    },
  })

  return (
    <Container
      fluid
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '1rem',
        backgroundImage:
          'radial-gradient(50% 50% at 5% 40%, #f4a460 0%, #8b4513 70%, #1a0f0a 100%)',
      }}
    >
      <Paper
        p="xl"
        radius="md"
        style={{
          width: '100%',
          maxWidth: '42rem',
          backdropFilter: 'blur(10px)',
          backgroundColor: 'rgba(0, 0, 0, 0.5)',
          border: '8px solid rgba(0, 0, 0, 0.1)',
        }}
      >
        <form
          onSubmit={(e) => {
            e.preventDefault()
            e.stopPropagation()
            form.handleSubmit()
          }}
        >
          <Stack gap="lg">
          <form.AppField name="fullName">
            {(field) => <field.TextField label="Full Name" />}
          </form.AppField>

          <form.AppField
            name="email"
            validators={{
              onBlur: ({ value }) => {
                if (!value || value.trim().length === 0) {
                  return 'Email is required'
                }
                if (!/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(value)) {
                  return 'Invalid email address'
                }
                return undefined
              },
            }}
          >
            {(field) => <field.TextField label="Email" />}
          </form.AppField>

          <form.AppField
            name="address.street"
            validators={{
              onBlur: ({ value }) => {
                if (!value || value.trim().length === 0) {
                  return 'Street address is required'
                }
                return undefined
              },
            }}
          >
            {(field) => <field.TextField label="Street Address" />}
          </form.AppField>

            <Grid>
              <Grid.Col span={{ base: 12, md: 4 }}>
                <form.AppField
                  name="address.city"
                  validators={{
                    onBlur: ({ value }) => {
                      if (!value || value.trim().length === 0) {
                        return 'City is required'
                      }
                      return undefined
                    },
                  }}
                >
                  {(field) => <field.TextField label="City" />}
                </form.AppField>
              </Grid.Col>
              <Grid.Col span={{ base: 12, md: 4 }}>
                <form.AppField
                  name="address.state"
                  validators={{
                    onBlur: ({ value }) => {
                      if (!value || value.trim().length === 0) {
                        return 'State is required'
                      }
                      return undefined
                    },
                  }}
                >
                  {(field) => <field.TextField label="State" />}
                </form.AppField>
              </Grid.Col>
              <Grid.Col span={{ base: 12, md: 4 }}>
                <form.AppField
                  name="address.zipCode"
                  validators={{
                    onBlur: ({ value }) => {
                      if (!value || value.trim().length === 0) {
                        return 'Zip code is required'
                      }
                      if (!/^\d{5}(-\d{4})?$/.test(value)) {
                        return 'Invalid zip code format'
                      }
                      return undefined
                    },
                  }}
                >
                  {(field) => <field.TextField label="Zip Code" />}
                </form.AppField>
              </Grid.Col>
            </Grid>

          <form.AppField
            name="address.country"
            validators={{
              onBlur: ({ value }) => {
                if (!value || value.trim().length === 0) {
                  return 'Country is required'
                }
                return undefined
              },
            }}
          >
            {(field) => (
              <field.Select
                label="Country"
                values={[
                  { label: 'United States', value: 'US' },
                  { label: 'Canada', value: 'CA' },
                  { label: 'United Kingdom', value: 'UK' },
                  { label: 'Australia', value: 'AU' },
                  { label: 'Germany', value: 'DE' },
                  { label: 'France', value: 'FR' },
                  { label: 'Japan', value: 'JP' },
                ]}
                placeholder="Select a country"
              />
            )}
          </form.AppField>

          <form.AppField
            name="phone"
            validators={{
              onBlur: ({ value }) => {
                if (!value || value.trim().length === 0) {
                  return 'Phone number is required'
                }
                if (
                  !/^(\+\d{1,3})?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$/.test(
                    value,
                  )
                ) {
                  return 'Invalid phone number format'
                }
                return undefined
              },
            }}
          >
            {(field) => (
              <field.TextField label="Phone" placeholder="123-456-7890" />
            )}
          </form.AppField>

            <Group justify="flex-end">
              <form.AppForm>
                <form.SubscribeButton label="Submit" />
              </form.AppForm>
            </Group>
          </Stack>
        </form>
      </Paper>
    </Container>
  )
}
