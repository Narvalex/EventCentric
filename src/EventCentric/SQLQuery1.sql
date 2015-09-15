select * from EventStore.Streams as s where s.StreamId = '9D696497-C0DB-4E39-9B94-A51400881F5E'
delete from EventStore.Streams where StreamId = '9D696497-C0DB-4E39-9B94-A51400881F5E'

use EmpresasReadModel 
go

select * from EmpresasQueue.SetValidation.Empresas 
where Nombre = 'Easy Trade'

select * from EventStore.Subscriptions


