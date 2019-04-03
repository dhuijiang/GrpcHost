protoc -I. ^
--include_imports ^
--include_source_info ^
--descriptor_set_out=customer_service_definition.pb ^
src/main/proto/Customer.proto