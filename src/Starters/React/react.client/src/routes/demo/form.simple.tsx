import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'
import { Container, Paper, Stack, Group } from '@mantine/core'

import { useAppForm } from '@/hooks/demo.form'

export const Route = createFileRoute('/demo/form/simple')({
  component: SimpleForm,
})

const schema = z.object({
  title: z.string().min(1, 'Title is required'),
  description: z.string().min(1, 'Description is required'),
})

function SimpleForm() {
  const form = useAppForm({
    defaultValues: {
      title: '',
      description: '',
    },
    validators: {
      onBlur: schema,
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
          'radial-gradient(50% 50% at 5% 40%, #add8e6 0%, #0000ff 70%, #00008b 100%)',
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
            <form.AppField name="title">
              {(field) => <field.TextField label="Title" />}
            </form.AppField>

            <form.AppField name="description">
              {(field) => <field.TextArea label="Description" />}
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
